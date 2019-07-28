using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[Serializable]
[PostProcess(typeof(ScanlineRenderer),PostProcessEvent.BeforeStack,"Scanline")]
public class Scanlines :PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Grayscale effect intensity.")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };
}

public sealed class ScanlineRenderer : PostProcessEffectRenderer<Scanlines>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/ScanLine"));
        sheet.properties.SetFloat("_Blend", settings.blend);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
