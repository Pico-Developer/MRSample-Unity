// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if GT_USE_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Microsoft.MixedReality.GraphicsTools
{
    /// <summary>
    /// Render pass implementation for the AcrylicBlur renderer feature
    /// </summary>

    class AcrylicBlurRenderPass : ScriptableRenderPass
    {   
        public bool setMaterialTexture = false;
        private string profilerLabel;
        private RTHandle target1;
        private RTHandle target2;
        private int downSample;
        private int passes;
        private string textureName;
        private Material blurMaterial;
        private Vector2 pixelSize;
        private RenderTexture providedTexture;
        private bool blur;
        private AcrylicFilterDual blurFilter;

        public AcrylicBlurRenderPass(string _profilerLabel, int _downSamplePasses, int _passes, Material material, string _textureName, bool _blur, RenderTexture _texture, AcrylicFilterDual _blurFilter)
        {
            profilerLabel = _profilerLabel;
            passes = _passes;
            textureName = _textureName;
            blurMaterial = material;
            providedTexture = _texture;

            downSample = 1;
            int i = _downSamplePasses;
            while (i > 0)
            {
                downSample *= 2;
                i--;
            }
            blur = _blur;
            blurFilter = _blurFilter;
        }

        private Vector4 info = Vector4.zero;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int width = cameraTextureDescriptor.width / downSample;
            int height = cameraTextureDescriptor.height / downSample;

            pixelSize = new Vector2(1.0f / width, 1.0f / height);

            info.x = cameraTextureDescriptor.vrUsage == VRTextureUsage.TwoEyes ? 1.0f : 0.5f;
            info.y = cameraTextureDescriptor.volumeDepth > 1 ? 1.0f : 0.5f;
            info.z = 1.0f;
            info.w = 1.0f;

            var descriptor = cameraTextureDescriptor;
            descriptor.width = width;
            descriptor.height = height;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            descriptor.useMipMap = false;
            descriptor.autoGenerateMips = false;

            RenderingUtils.ReAllocateHandleIfNeeded(
                ref target1,
                descriptor,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: profilerLabel + "RenderTarget1");
            RenderingUtils.ReAllocateHandleIfNeeded(
                ref target2,
                descriptor,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: profilerLabel + "RenderTarget2");
            ConfigureTarget(target1);

            if (providedTexture != null)
            {
                if (width != providedTexture.width || height != providedTexture.height)
                {
                    providedTexture.Release();
                    providedTexture.width = width;
                    providedTexture.height = height;
                    providedTexture.Create();
                }

                if (setMaterialTexture)
                    cmd.SetGlobalTexture(textureName, providedTexture);
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerLabel);
            cmd.Clear();

            RenderTargetIdentifier handle = providedTexture == null
                ? target1.nameID
                : new RenderTargetIdentifier(providedTexture);
            var renderer = renderingData.cameraData.renderer;

            cmd.SetGlobalVector("_AcrylicInfo", info);

            if (downSample == 1)
            {
                cmd.Blit(renderer.cameraColorTargetHandle.nameID, handle);
            }
            else if (downSample == 2)
            {
                cmd.SetGlobalVector("_AcrylicBlurOffset", Vector2.zero);
                LocalBlit(cmd, renderer.cameraColorTargetHandle.nameID, handle, blurMaterial);
            }
            else
            {
                cmd.SetGlobalVector("_AcrylicBlurOffset", 0.25f * pixelSize);
                LocalBlit(cmd, renderer.cameraColorTargetHandle.nameID, handle, blurMaterial);
            }

            if (blur)
            {
                if (blurFilter == null || providedTexture==null)
                {
                    QueueBlurPasses(cmd, BlurWidths(passes));
                } 
                else
                {                    
                    blurFilter.QueueBlur(cmd, providedTexture, passes);
                }
            }

            if (providedTexture==null && setMaterialTexture)
            {
                cmd.SetGlobalTexture(textureName, target1.nameID);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        private void QueueBlurPasses(CommandBuffer cmd, float[] widths)
        {
            for (int i = 0; i < widths.Length; i++)
            {
                cmd.SetGlobalVector("_AcrylicBlurOffset", (0.5f + widths[i]) * pixelSize);
                if (providedTexture != null && i == widths.Length - 1)
                {
                    LocalBlit(cmd, target1.nameID, new RenderTargetIdentifier(providedTexture), blurMaterial);
                }
                else if (providedTexture != null && i == 0)
                {
                    LocalBlit(cmd, new RenderTargetIdentifier(providedTexture), target1.nameID, blurMaterial);
                }
                else
                {
                    LocalBlit(cmd, target1.nameID, target2.nameID, blurMaterial);
                    SwapTempTargets();
                }
            }
        }

        //TODO:  replace with new URP Blit() when that's working with multiview
        private void LocalBlit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier target, Material material)
        {
            cmd.SetRenderTarget(target);
            cmd.SetGlobalTexture("_AcrylicBlurSource", source);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);
        }

        private void SwapTempTargets()
        {
            var rttmp = target1;
            target1 = target2;
            target2 = rttmp;
        }

        public void Dispose()
        {
            target1?.Release();
            target1 = null;
            target2?.Release();
            target2 = null;
        }


        public static float[] BlurWidths(int passes)
        {
            switch (passes)
            {
                case 2:
                    return new float[] { 0.0f, 0.0f };

                case 3:
                    return new float[] { 0.0f, 1.0f, 1.0f };

                case 4:
                    return new float[] { 0.0f, 1.0f, 1.0f, 2.0f };

                case 5:
                    return new float[] { 0.0f, 1.0f, 2.0f, 2.0f, 3.0f };

                case 6:
                    return new float[] { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 4.0f, 5.0f };

                default:
                    return new float[] { 0.0f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 7.0f, 8.0f, 9.0f, 10.0f };
            }
        }
    }
}
#endif // GT_USE_URP
