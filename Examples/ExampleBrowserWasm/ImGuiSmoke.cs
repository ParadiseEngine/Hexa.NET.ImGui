namespace ExampleBrowserWasm
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    /// <summary>
    /// Headless checks for the parts of the binding a new platform can get wrong.
    /// </summary>
    /// <remarks>
    /// The wasm C ABI passes multi-field structs such as ImVec2, ImVec4 and ImTextureRef through
    /// a pointer, so every by-value struct argument must reach cimgui intact through the
    /// binding's function pointers. A mismatch reads back plausible nonsense instead of crashing,
    /// hence exact comparisons.
    /// </remarks>
    public static unsafe class ImGuiSmoke
    {
        private const ulong ImageTextureId = 42;

        public static void Run(Action<string> log)
        {
            ImGuiContextPtr context = ImGui.CreateContext();
            try
            {
                ImGui.SetCurrentContext(context);
                ImGuiIOPtr io = ImGui.GetIO();
                io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures | ImGuiBackendFlags.RendererHasVtxOffset;
                io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
                io.DisplaySize = new Vector2(800, 600);
                io.DeltaTime = 1f / 60f;
                io.IniFilename = null;
                log($"Dear ImGui {ImGui.GetVersionS()}");

                Expect(ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0, 0, 1)) == 0xFF0000FFu,
                    "ColorConvertFloat4ToU32(Vector4) by value");

                var statuses = new List<ImTextureStatus>();
                uint rootId = 0, leftId = 0, rightId = 0;
                for (int frame = 0; frame < 3; frame++)
                {
                    ImGui.NewFrame();
                    if (frame == 0)
                    {
                        rootId = ImGui.GetID("root");
                        ImGuiP.DockBuilderAddNode(rootId, ImGuiDockNodeFlags.None);
                        ImGuiP.DockBuilderSetNodeSize(rootId, new Vector2(600, 400));
                        ImGuiP.DockBuilderSplitNode(rootId, ImGuiDir.Left, 0.25f, &leftId, &rightId);
                        ImGuiP.DockBuilderDockWindow("left", leftId);
                        ImGuiP.DockBuilderDockWindow("right", rightId);
                        ImGuiP.DockBuilderFinish(rootId);
                    }
                    ImGui.DockSpace(rootId, new Vector2(600, 400));

                    ImGui.Begin("left");
                    bool leftDocked = ImGui.IsWindowDocked();
                    uint leftDockId = ImGui.GetWindowDockID();
                    ImGui.TextUnformatted(frame == 0 ? "hello" : "XYZ@#");
                    ImGui.End();

                    ImGui.Begin("right");
                    ImGui.End();

                    ImGui.SetNextWindowPos(new Vector2(40, 48));
                    ImGui.SetNextWindowSize(new Vector2(140, 100));
                    ImGui.Begin("abi", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking);
                    Vector2 position = ImGui.GetWindowPos();
                    Vector2 size = ImGui.GetWindowSize();
                    float frameHeight = ImGui.GetFrameHeight();
                    ImGui.Image(new ImTextureRef(null, new ImTextureID(ImageTextureId)), new Vector2(16, 16));
                    ImGui.End();

                    ImGui.Render();
                    ImDrawDataPtr drawData = ImGui.GetDrawData();
                    statuses.Add(AcknowledgeTextures(drawData));

                    if (frame == 2)
                    {
                        Expect(leftDocked && leftDockId == leftId, $"DockBuilder docked 'left' into node {leftId} (got {leftDockId})");
                        Expect(position == new Vector2(40, 48), $"SetNextWindowPos/GetWindowPos round trip (got {position})");
                        Expect(size == new Vector2(140, 100), $"SetNextWindowSize/GetWindowSize round trip (got {size})");
                        Expect(frameHeight > 0, $"float return GetFrameHeight (got {frameHeight})");
                        Expect(HasDrawCommandFor(drawData, ImageTextureId), "Image(ImTextureRef) by value reaches the draw list");
                    }
                }

                Expect(statuses[0] == ImTextureStatus.WantCreate && statuses[2] == ImTextureStatus.Ok,
                    $"atlas texture protocol {string.Join(" -> ", statuses)}");

                // The atlas initializes its loader on first build; FreeType rasterized the atlas checked above.
                string fontLoader = new((sbyte*)io.Fonts.FontLoaderName);
                Expect(fontLoader.StartsWith("FreeType", StringComparison.Ordinal), $"atlas built with the FreeType font loader (got '{fontLoader}')");
            }
            finally
            {
                ImGui.DestroyContext(context);
            }

            void Expect(bool condition, string what)
            {
                if (!condition)
                {
                    throw new InvalidOperationException($"FAILED: {what}");
                }
                log($"ok: {what}");
            }
        }

        /// <summary>Plays the renderer's part of the 1.92 texture protocol; returns the atlas status seen.</summary>
        private static ImTextureStatus AcknowledgeTextures(ImDrawDataPtr drawData)
        {
            ImVector<ImTextureDataPtr>* textures = drawData.Handle->Textures;
            ImTextureStatus atlasStatus = textures->Data[0].Status;
            for (int i = 0; i < textures->Size; i++)
            {
                ImTextureDataPtr texture = textures->Data[i];
                switch (texture.Status)
                {
                    case ImTextureStatus.WantCreate:
                        texture.SetTexID(new ImTextureID((ulong)texture.UniqueID + 1));
                        texture.SetStatus(ImTextureStatus.Ok);
                        break;
                    case ImTextureStatus.WantUpdates:
                        texture.SetStatus(ImTextureStatus.Ok);
                        break;
                    case ImTextureStatus.WantDestroy:
                        texture.SetTexID(ImTextureID.Null);
                        texture.SetStatus(ImTextureStatus.Destroyed);
                        break;
                }
            }
            return atlasStatus;
        }

        private static bool HasDrawCommandFor(ImDrawDataPtr drawData, ulong textureId)
        {
            for (int i = 0; i < drawData.CmdLists.Size; i++)
            {
                ImDrawListPtr list = drawData.CmdLists[i];
                for (int j = 0; j < list.CmdBuffer.Size; j++)
                {
                    if (list.CmdBuffer[j].GetTexID().Handle == textureId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
