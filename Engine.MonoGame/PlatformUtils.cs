using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using Engine.Core;
using Engine.Core.Exceptions;
using Engine.Core.Platform;

using MonoGame.Framework.Utilities;

namespace Engine.MonoGame
{
    public static class PlatformUtils
    {
        public static EngineRuntime GetRuntimeInfo()
        {
            RuntimePlatform platform = GetRuntimePlatform();
            ApplicationMode mode = GetApplicationMode();
            GraphicsAPI api = GetGraphicsAPI();
            return new EngineRuntime(platform, mode, api);
        }

        private static RuntimePlatform GetRuntimePlatform()
        {
            MonoGamePlatform platform = PlatformInfo.MonoGamePlatform;
            switch (platform)
            {
                case MonoGamePlatform.Android:
                    return RuntimePlatform.Android;

                case MonoGamePlatform.iOS:
                    return RuntimePlatform.iOS;

                case MonoGamePlatform.Windows:
                case MonoGamePlatform.WindowsDX12:
                    return RuntimePlatform.Windows;

                case MonoGamePlatform.WebGL:
                    return RuntimePlatform.WebGL;

                case MonoGamePlatform.XboxOne:
                    return RuntimePlatform.XBox;

                case MonoGamePlatform.PlayStation4:
                case MonoGamePlatform.PlayStation5:
                    return RuntimePlatform.PlayStation;

                case MonoGamePlatform.NintendoSwitch:
                    return RuntimePlatform.NintendoSwitch;

                case MonoGamePlatform.DesktopGL:
                case MonoGamePlatform.DesktopVK:
                { 
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return RuntimePlatform.Windows;
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        return RuntimePlatform.Linux;
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        return RuntimePlatform.MacOS;
                    }

                    break;
                }
            }

            throw new PlatformNotSupportedException(
                $"Unsupported platform! " +
                $"MonoGame platform: {platform}; " +
                $"OS Description: {RuntimeInformation.OSDescription}");
        }

        private static ApplicationMode GetApplicationMode()
        {
            // Don't have a way of detecting this yet
            return ApplicationMode.Windowed;
        }

        private static GraphicsAPI GetGraphicsAPI()
        {
            return PlatformInfo.GraphicsBackend switch
            {
                GraphicsBackend.DirectX => GraphicsAPI.DirectX,
                GraphicsBackend.OpenGL => GraphicsAPI.OpenGL,
                GraphicsBackend.Vulkan => GraphicsAPI.Vulkan,
                GraphicsBackend.Metal => GraphicsAPI.Metal,
                GraphicsBackend.DirectX12 => GraphicsAPI.DirectX12,
                _ => throw new PlatformNotSupportedException("GraphicsBackend is not supported by this game")
            };
        }
    }
}
