﻿using System;
using System.Collections.Generic;
using System.IO;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class PlanetExtensionSystem
    {
        public static List<PlanetExtensionStorage> extensions = new List<PlanetExtensionStorage>();
        public static TypeRegistry<IPlanetExtension, PlanetExtensionStorage> registry = new TypeRegistry<IPlanetExtension, PlanetExtensionStorage>();

        internal static Dictionary<int, byte[]> pendingData = new Dictionary<int, byte[]>();
        internal static Action<PlanetData> onInitNewPlanet;
        
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(PlanetExtensionHooks));
        }


        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void load()
        {
            CommonAPIPlugin.registries.Add($"{CommonAPIPlugin.ID}:PlanetExtensionRegistry", registry);
        }
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(PlanetExtensionSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(PlanetExtensionSystem)})]");
            }
        }


        public static void InitOnLoad()
        {
            if (Loaded)
            {
                CommonAPIPlugin.logger.LogInfo("Loading planet extension system");
                GameData data = GameMain.data;

                extensions.Clear();
                extensions.Capacity = registry.data.Count + 1;
                extensions.Add(null);
                for (int i = 1; i < registry.data.Count; i++)
                {
                    PlanetExtensionStorage storage = new PlanetExtensionStorage();
                    storage.InitOnLoad(data, i);
                    extensions.Add(storage);
                }
            }
        }

        public static void InitNewPlanet(PlanetData planet)
        {
            for (int i = 1; i < registry.data.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.InitNewPlanet(planet);
            }
            
            onInitNewPlanet?.Invoke(planet);
        }


        public static void CreateEntityComponents(PlanetFactory factory, int entityId, PrefabDesc desc, int prebuildId)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                IPlanetExtension extension = storage.GetExtension(factory);
                if (extension is IComponentStateListener listener)
                {
                    listener.OnLogicComponentsAdd(entityId, desc, prebuildId);
                }
            }

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                IPlanetExtension extension = storage.GetExtension(factory);
                if (extension is IComponentStateListener listener)
                {
                    listener.OnPostlogicComponentsAdd(entityId, desc, prebuildId);
                }
            }
        }

        public static void RemoveEntityComponents(PlanetFactory factory, int entityId)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                IPlanetExtension extension = storage.GetExtension(factory);
                if (extension is IComponentStateListener listener)
                {
                    listener.OnLogicComponentsRemove(entityId);
                }
            }
        }

        public static void DrawUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];

                storage.DrawUpdate(factory);
            }
        }

        public static void PowerUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];

                storage.PowerUpdate(factory);
            }
        }

        public static void PreUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.PreUpdate(factory);
            }
        }

        public static void Update(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.Update(factory);
            }
        }

        public static void PostUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.PostUpdate(factory);
            }
        }

        public static void PowerUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < extensions.Count; j++)
                {
                    PlanetExtensionStorage storage = extensions[j];
                    if (storage.PowerUpdateSupportsMultithread()) return;

                    storage.PowerUpdate(factory);
                }
            }
        }

        public static void PreUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < extensions.Count; j++)
                {
                    PlanetExtensionStorage storage = extensions[j];
                    if (storage.PreUpdateSupportsMultithread()) return;

                    storage.PreUpdate(factory);
                }
            }
        }

        public static void UpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < extensions.Count; j++)
                {
                    PlanetExtensionStorage storage = extensions[j];
                    if (storage.UpdateSupportsMultithread()) return;

                    storage.Update(factory);
                }
            }
        }

        public static void PostUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;


                for (int j = 1; j < extensions.Count; j++)
                {
                    PlanetExtensionStorage storage = extensions[j];
                    if (storage.PostUpdateSupportsMultithread()) return;

                    storage.PostUpdate(factory);
                }
            }
        }

        public static void PowerUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.PowerUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void PreUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.PreUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void UpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.UpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void PostUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                PlanetExtensionStorage storage = extensions[i];
                storage.PostUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }


        public static void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();
            bool wasLoaded = r.ReadBoolean();

            if (wasLoaded)
            {
                registry.ImportAndMigrate(extensions, r);
            }
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(0);
            w.Write(Loaded);

            if (Loaded)
            {
                registry.ExportContainer(extensions, w);
            }
        }
    }
}