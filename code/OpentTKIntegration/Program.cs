using System;

using OpenTK.Windowing.Common.Input;

using OpentTKIntegration.Components;

namespace OpentTKIntegration {
    class Program {
        static void Main(string[] args) {
            var gameSettings = new OpenTK.Windowing.Desktop.GameWindowSettings {
                RenderFrequency = 60,
                UpdateFrequency = 60
            };

            var nativeSettings = new OpenTK.Windowing.Desktop.NativeWindowSettings {
                Size = new() { X = 500, Y = 500 },
                API = OpenTK.Windowing.Common.ContextAPI.OpenGL,
                Title = "SquidLib Sharp OpenTK Demo",

            };
            new MainWindow(gameSettings, nativeSettings).Run();
        }
    }
}
