using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PerformanceLogger : MonoBehaviour
{
    public GridSpawner spawner;
    public string testName = "T1_All_OFF";
    public float warmupDuration = 2.0f;
    public float testDuration = 10.0f;
    public int maxTargetFps = 240;
    public float emergencyFrameTimeLimit = 0.5f;

    private ProfilerRecorder drawCallsRecorder;
    private ProfilerRecorder batchesRecorder;
    private ProfilerRecorder setPassRecorder;
    private ProfilerRecorder trianglesRecorder;
    private ProfilerRecorder mainThreadTimeRecorder;

    private readonly System.Diagnostics.Stopwatch renderStopwatch = new System.Diagnostics.Stopwatch();
    private double lastRenderCpuMs;

    private readonly List<string> csvLines = new List<string>();
    private float warmupTimer;
    private float logTimer;
    private int consecutiveStalls;
    private bool isFinished;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxTargetFps;
    }

    void OnEnable()
    {
        csvLines.Clear();
        csvLines.Add("Time_s,FPS,CPU_FrameTime_ms,RenderCPU_ms,DrawCalls,Batches,SetPassCalls,Triangles");

        drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        setPassRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
        trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

        if (drawCallsRecorder.Valid) drawCallsRecorder.Dispose();
        if (batchesRecorder.Valid) batchesRecorder.Dispose();
        if (setPassRecorder.Valid) setPassRecorder.Dispose();
        if (trianglesRecorder.Valid) trianglesRecorder.Dispose();
        if (mainThreadTimeRecorder.Valid) mainThreadTimeRecorder.Dispose();
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game) return;
        renderStopwatch.Restart();
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Game) return;
        renderStopwatch.Stop();
        lastRenderCpuMs = renderStopwatch.Elapsed.TotalMilliseconds;
    }

    void Update()
    {
        if (isFinished) return;

        float dt = Time.unscaledDeltaTime;

        if (Time.frameCount > 5 && CheckEmergencyStop(dt)) return;

        if (spawner != null && !spawner.IsReady) return;

        if (warmupTimer < warmupDuration)
        {
            warmupTimer += dt;
            return;
        }

        logTimer += dt;
        RecordFrame(dt);

        if (logTimer >= testDuration)
        {
            isFinished = true;
            SaveToCsv();
            StopSession();
        }
    }

    private bool CheckEmergencyStop(float dt)
    {
        if (dt > emergencyFrameTimeLimit)
        {
            consecutiveStalls++;
            if (consecutiveStalls >= 2)
            {
                isFinished = true;
                Debug.LogWarning("Safety stop triggered due to high frame time.");
                StopSession();
                return true;
            }
        }
        else
        {
            consecutiveStalls = 0;
        }

        return false;
    }

    private void RecordFrame(float dt)
    {
        float fps = dt > 0f ? 1.0f / dt : 0f;
        double cpuMs = mainThreadTimeRecorder.LastValue * 1e-6;

        long drawCalls = drawCallsRecorder.LastValue;
        long batches = batchesRecorder.LastValue;
        long setPass = setPassRecorder.LastValue;
        long triangles = trianglesRecorder.LastValue;

#if UNITY_EDITOR
        if (drawCalls == 0) drawCalls = UnityStats.drawCalls;
        if (batches == 0) batches = UnityStats.drawCalls;
        if (setPass == 0) setPass = UnityStats.setPassCalls;
        if (triangles == 0) triangles = UnityStats.triangles;
#endif

        string line = string.Format(
            CultureInfo.InvariantCulture,
            "{0:F3},{1:F1},{2:F3},{3:F4},{4},{5},{6},{7}",
            logTimer, fps, cpuMs, lastRenderCpuMs, drawCalls, batches, setPass, triangles
        );

        csvLines.Add(line);
    }

    private void SaveToCsv()
    {
        string filePath = Path.Combine(
            Application.dataPath,
            "../",
            $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        );
        File.WriteAllLines(filePath, csvLines);
        Debug.Log($"Saved: {filePath}");
    }

    private void StopSession()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}