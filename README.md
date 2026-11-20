# Renderelési módok és árnyalók performancia vizsgálata Unity környezetben

Szakdolgozati projekt.

## A kutatás célja és témaleírása

A grafikus alkalmazások esetében egy lényeges szempont, hogy a kép részletessége, megfelelő minőségű megjelenítése mellett az annak előállításához szükséges számítási idő minél kevesebb legyen. A szakdolgozat azt vizsgálja, hogy Unity Engine-ben milyen renderelési módok érhetők el, továbbá hogy pixelárnyalók (shader-ek) alkalmazásával hogyan lehet a teljesítményt javítani. Ehhez kialakításra kerülnek minta színterek. Részletezésre kerül a mérések módja, az abból kapott eredmények kiértékelése. Az elemzésekhez és az optimalizáláshoz az alkalmazások C# programozási nyelven, Unity környezetben készülnek.

## Technológiai verem

* **Játékmotor:** Unity Engine 
* **Programozási nyelv:** C#
* **Grafikus programozás:** HLSL / Unity Shader Graph
* **Verziókezelés:** Git

## A projekt tervezett felépítése



## Futtatási útmutató


## Teljesítményvizsgálat
A teljesítményvizsgálathoz három különböző, specifikus hardveres terhelést generáló színtér kerül kialakításra. A mérések determinisztikusságát egy fix kameraútvonalat bejáró, és az adatokat naplózó C# szkript biztosítja.

1. **Geometria-intenzív színtér Vertex Bound Test**
   * **Fókusz:** A csúcspont-feldolgozás és a Draw Call-ok kezelése.
   * **Kialakítás:** Extrém magas poligonszámú modellek sűrű elhelyezése, alapvető textúrákkal és minimális világítással.
   * **Cél:** A GPU Instancing, a Static Batching és a culling rendszerek hatékonyságának mérése.

2. **Világítás- és Árnyék-intenzív színtér Fragment Bound Test**
   * **Fókusz:** A Forward és Deferred rendering fénykezelési különbségeinek kimutatása.
   * **Kialakítás:** Alacsony poligonszámú, egyszerű geometria, viszont nagyszámú dinamikus fényforrás egymást átfedő hatássugarakkal és valós idejű árnyékvetéssel.

3. **Komplex Árnyalók és Overdraw színtér Shader Test**
   * **Fókusz:** A memóriasávszélesség és a matematikai számításigény mérése.
   * **Kialakítás:** Szándékosan nehéz, optimalizálatlan pixel shaderek, valamint nagyszámú, egymást átfedő áttetsző (transparent) felület.
   * **Cél:** Az Overdraw és a mélységpuffer megkerüléséből fakadó teljesítményromlás számszerűsítése.

## Renderelési módok áttekintése

A vizsgálat a Unity által kínált három fő renderelési csővezetéket és az azokon belüli megjelenítési útvonalakat hasonlítja össze. Fontos mérnöki szempont, hogy a már elavultnak minősített Built-In Render Pipeline a mérések során referenciapontként szolgál. Ennek segítségével egzakt módon, számszerűsítve kimutatható a modern csővezetékekre való átállás teljesítménybeli nyeresége.

| Renderelési Csővezeték | Támogatott Útvonalak | Fénykezelés jellemzője | Fő optimalizációs cél |
| :--- | :--- | :--- | :--- |
| **Built-in Render Pipeline (BRP)** | Forward, Deferred | Forward: Objektumonként szigorúan korlátozott fényszám. Deferred: Képernyőtér alapú. | *Referenciapont.* Régebbi, "legacy" architektúra, hamarosan kivezetésre kerül. |
| **Universal Render Pipeline (URP)** | Forward, Forward+, Deferred | Forward+: Csempézett fénykezelés, megemelt fénykorlátokkal. | Magas skálázhatóság, fókuszban az SRP Batcher hatékonysága. |
| **High Definition Render Pipeline (HDRP)**| Forward, Deferred | Fizika-alapú (PBR), Compute Shader vezérelt komplex fénykezelés. | Magas teljesítményű asztali GPU-kra tervezve, vizuális hűség maximalizálása. |

### Megjelenítési Útvonalak a fókuszban:
A kutatás kiemelt eleme a **Forward** és a **Deferred** rendering összehasonlítása. Míg a Forward rendering esetén a fények számítási költsége geometriafüggő a sok fény drasztikus Draw Call és feldolgozási idő növekedést okoz, addig a Deferred rendering a világítást a G-Buffer felépítése után, a képernyőtérben számolja, így a nagyszámú dinamikus fényforrás kezelése sokkal hatékonyabbá válik memóriasávszélesség terhére.

## Hivatkozások

**1. Renderelési Csővezetékek és Fénykezelés:**
* Unity Manual: *Understanding the Render Pipelines.* https://docs.unity3d.com/Manual/render-pipelines.html
* Unity Manual: *Rendering Paths.* https://docs.unity3d.com/Manual/RenderingPaths.html
* Unity URP Documentation: *Lighting.* https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/lighting.html

**2. Teljesítménymérés és Profilozás:**
* Unity Manual: *Optimizing Graphics Performance* https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html
* Unity Manual: *Profiler overview* https://docs.unity3d.com/Manual/Profiler.html
* Unity Manual: *The Frame Debugger* https://docs.unity3d.com/Manual/FrameDebugger.html
* Unity Profile Analyzer: *Profile Analyzer Package* https://docs.unity3d.com/Packages/com.unity.performance.profile-analyzer@latest/index.html

**3. Geometria, Draw Call-ok és Overdraw:**
* Unity Manual: *Optimizing Draw Calls* https://docs.unity3d.com/Manual/optimizing-draw-calls.html
* Unity Manual: *GPU Instancing* https://docs.unity3d.com/Manual/GPUInstancing.html

**4. Árnyalók programozása és optimalizálása:**
* Unity Manual: *Shader Performance* https://docs.unity3d.com/Manual/SL-ShaderPerformance.html

## Résztvevők

* **Szerző:** Héring István ZL3ACR
* **Hivatalos témavezető:** Dr. Túri József
* **Szakmai konzulens:** Piller Imre
