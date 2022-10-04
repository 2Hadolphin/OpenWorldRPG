using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Return.Cameras
{
    public static class URPCameraExtension
    {
        public static void Copy(this Camera from,Camera target)
        {
            target.transparencySortMode = from.transparencySortMode;
            target.transparencySortAxis = from.transparencySortAxis;

            target.opaqueSortMode = from.opaqueSortMode;

            target.allowDynamicResolution = from.allowDynamicResolution;
            target.allowHDR = from.allowHDR;
            target.allowMSAA = from.allowMSAA;
            target.aspect = from.aspect;
            target.backgroundColor = from.backgroundColor;

            target.cameraType = from.cameraType;
            target.clearFlags = from.clearFlags;
            target.clearStencilAfterLightingPass = from.clearStencilAfterLightingPass;
            target.cullingMask = from.cullingMask;
            target.cullingMatrix = from.cullingMatrix;
            target.depth = from.depth;
            target.depthTextureMode = from.depthTextureMode;
            target.eventMask = from.eventMask;
            target.farClipPlane = from.farClipPlane;
            target.fieldOfView = from.fieldOfView;
            target.focalLength = from.focalLength;
            target.forceIntoRenderTexture = from.forceIntoRenderTexture;
            target.gateFit = from.gateFit;
            target.layerCullDistances = from.layerCullDistances;
            target.layerCullSpherical = from.layerCullSpherical;
            target.lensShift = from.lensShift;
            target.nearClipPlane = from.nearClipPlane;
            target.nonJitteredProjectionMatrix = from.nonJitteredProjectionMatrix;

            target.orthographic = from.orthographic;
            target.orthographicSize = from.orthographicSize;
            target.overrideSceneCullingMask = from.overrideSceneCullingMask;

            target.pixelRect = from.pixelRect;
            target.projectionMatrix = from.projectionMatrix;
            target.rect = from.rect;
            target.renderingPath = from.renderingPath;

            target.sensorSize = from.sensorSize;
            target.stereoConvergence = from.stereoConvergence;
            target.stereoSeparation = from.stereoSeparation;
            target.stereoTargetEye = from.stereoTargetEye;
            target.targetDisplay = from.targetDisplay;
            target.targetTexture = from.targetTexture;
            target.useJitteredProjectionMatrixForTransparentRendering = from.useJitteredProjectionMatrixForTransparentRendering;
            target.useOcclusionCulling = from.useOcclusionCulling;
            target.usePhysicalProperties = from.usePhysicalProperties;

        }
    }
}