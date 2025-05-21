---
uid: um-profiler-ui
---

# UI and UI Details Profiler

The UI and UI Details Profiler modules provide information on how much time and resources Unity spends laying out and rendering the user interface within your application. You can use this module to understand how Unity handles UI batching for your application, including why and how it batches objects. You can also use this module to find out which part of the UI is responsible for slow performance, or to preview the UI while you scrub the timeline.

To open the Profiler window, go to **Window &gt; Analysis &gt; Profiler**. For more information on how to use the Profiler window, refer to [The Profiler window](ProfilerWindow).

![The UI and UI Details Profiler module](../uploads/Main/ui-profiler-module.png)

## Chart categories
The UI and UI Details Profiler modules’ charts are divided into five categories. To change the order of the categories in the chart, you can drag and drop them in the chart’s legend. You can also click a category’s colored legend to toggle its display.


|**Chart**||**Description**|
|---|---|---|
|**UI Profiler module**|||
||Layout|How much time Unity has spent performing the layout pass for the UI. This includes calculations done by [HorizontalLayoutGroup](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.HorizontalLayoutGroup.html), [VerticalLayoutGroup](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.VerticalLayoutGroup.html), and [GridLayoutGroup](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.GridLayoutGroup.html).|
||Render|How much time the UI has spent doing its portion of rendering. This is either the cost of rendering directly to the graphics device or rendering to the main render queue.|
|**UI Details Profile module**|||
||Batches|Displays the total number of draw calls that are batched together. |
||Vertices|The total number of vertices that are used to render a section of UI. |
||Markers|Displays event markers. Unity records markers when the user interacts with the UI (for example, a button click, or a slider value change) and then draws them as vertical lines and labels on the chart.|

## Module details pane
When you select the UI or the UI Details Profiler module, the module details pane at the bottom of the Profiler window displays more details on the UI in your application. You can use it to inspect the profiling information about the UI objects in your application. The pane is divided into the following columns:

|**Column**|**Description**|
|---|---|
|**Object**|A list of UI canvases your application used during the period profiled. Double click on a row to highlight the matching object in the Scene.|
|**Self Batch Count**|How many batches Unity generated for the canvas.|
|**Cumulative Batch Count**|How many batches Unity generated for the canvas and all of its nested canvases|
|**Self Vertex Count**|How many vertices this canvas is rendering.|
|**Cumulative Vertex Count**|How many vertices this canvas and nested canvases are rendering|
|**Batch Breaking Reason**|Why Unity split the batch. Sometimes Unity might not be able to batch objects together. Common reasons include: <br/><br/>**Not Coplanar With Canvas**, where the batching needs the object’s rect transform to be coplanar (unrotated) with the canvas. <br/>**CanvasInjectionIndex**, where a CanvasGroup component is present and forces a new batch, such as when it displays the drop down list of a combo box on top of the rest.<br/>**Different Material Instance, Rect clipping, Texture, or A8TextureUsage**, where Unity can only batch together objects with identical materials, masking, textures, and texture alpha channel usage.|
|**GameObject Count**|How many GameObjects are part of this batch|
|**GameObjects**|The list of GameObjects in the batch.|

When you select a UI object from the list, a preview of it appears on the right hand side of the pane. Above the preview there are the following options in the toolbar:

* **Detach:** Select this button to open the UI canvas in a separate window. To reattach the window, close it.
* **Preview background:** Use the dropdown to change the color of the preview background. You can choose from **Checkerboard**, **Black**, or **White**. This is useful if your UI has a particularly light or dark color scheme.
* **Preview type:** Use the dropdown to select from **Standard**, **Overdraw**, or **Composite Overdraw**.

## Additional resources

* [Profiler window introduction](ProfilerWindow)
* [Profiling your application](profiler-profiling-applications)

