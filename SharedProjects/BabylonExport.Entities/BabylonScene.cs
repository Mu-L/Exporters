using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Babylon2GLTF;
using GLTFExport.Entities;

namespace BabylonExport.Entities
{
    [DataContract]
    public class BabylonScene
    {
        [DataMember]
        public BabylonProducer producer { get; set; }

        [DataMember]
        public bool autoClear { get; set; }

        [DataMember]
        public float[] clearColor { get; set; }

        [DataMember]
        public float[] ambientColor { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string environmentTexture { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? createDefaultSkybox { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public float? skyboxBlurLevel { get; set; }

        [DataMember]
        public int fogMode { get; set; }

        [DataMember]
        public float[] fogColor { get; set; }

        [DataMember]
        public float fogStart { get; set; }

        [DataMember]
        public float fogEnd { get; set; }

        [DataMember]
        public float fogDensity { get; set; }

        [DataMember]
        public float[] gravity { get; set; }

        [DataMember]
        public string physicsEngine { get; set; }

        [DataMember]
        public bool physicsEnabled { get; set; }

        [DataMember]
        public float[] physicsGravity { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public BabylonCamera[] cameras { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public string activeCameraID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BabylonLight[] lights { get; set; }

        [DataMember]
        public BabylonGeometries geometries { get; set; }

        [DataMember]
        public BabylonMesh[] meshes { get; set; }

        [DataMember]
        public BabylonSound[] sounds { get; set; }

        [DataMember]
        public BabylonMaterial[] materials { get; set; }

        [DataMember]
        public BabylonMultiMaterial[] multiMaterials { get; set; }

        [DataMember]
        public BabylonParticleSystem[] particleSystems { get; set; }

        [DataMember]
        public BabylonLensFlareSystem[] lensFlareSystems { get; set; }

        [DataMember]
        public BabylonShadowGenerator[] shadowGenerators { get; set; }

        [DataMember]
        public BabylonSkeleton[] skeletons { get; set; }

        [DataMember]
        public BabylonActions actions { get; set; }

        [DataMember]
        public object metadata { get; set; }

        [DataMember]
        public bool workerCollisions { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BabylonMorphTargetManager[] morphTargetManagers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<BabylonAnimationGroup> animationGroups { get; set; }

        public BabylonVector3 MaxVector { get; set; }
        public BabylonVector3 MinVector { get; set; }

        public int TimelineFramesPerSecond { get; set; }
        public int TimelineStartFrame { get; set; }
        public int TimelineEndFrame { get; set; }

        public string OutputPath { get; private set; }

        public List<BabylonMesh> MeshesList { get; private set; }
        public List<BabylonSound> SoundsList { get; private set; }
        public List<BabylonCamera> CamerasList { get; private set; }
        public List<BabylonLight> LightsList { get; private set; }
        public List<BabylonMaterial> MaterialsList { get; private set; }
        public List<BabylonMultiMaterial> MultiMaterialsList { get; private set; }
        public List<BabylonShadowGenerator> ShadowGeneratorsList { get; private set; }
        public List<BabylonSkeleton> SkeletonsList { get; private set; }
        public List<BabylonMorphTargetManager> MorphTargetManagersList { get; private set; }
        public Dictionary<string, BabylonNode> NodeMap { get; private set; }
        public Dictionary<IBabylonExtensionExporter,Type> BabylonToGLTFExtensions { get; private set; }

        public BabylonScene(string outputPath)
        {
            OutputPath = outputPath;

            MeshesList = new List<BabylonMesh>();
            MaterialsList = new List<BabylonMaterial>();
            CamerasList = new List<BabylonCamera>();
            LightsList = new List<BabylonLight>();
            MultiMaterialsList = new List<BabylonMultiMaterial>();
            ShadowGeneratorsList = new List<BabylonShadowGenerator>();
            SkeletonsList = new List<BabylonSkeleton>();
            SoundsList = new List<BabylonSound>();
            MorphTargetManagersList = new List<BabylonMorphTargetManager>();
            NodeMap = new Dictionary<string, BabylonNode>();
            BabylonToGLTFExtensions = new Dictionary<IBabylonExtensionExporter,Type>();

            // Default values
            autoClear = true;
            clearColor = new[] { 0.2f, 0.2f, 0.3f };
            ambientColor = new[] { 0f, 0f, 0f };
            gravity = new[] { 0f, 0f, -0.9f };

            MaxVector = new BabylonVector3 { X = float.MinValue, Y = float.MinValue, Z = float.MinValue };
            MinVector = new BabylonVector3 { X = float.MaxValue, Y = float.MaxValue, Z = float.MaxValue };
        }

        public void Prepare(bool generateDefaultLight = true, bool generateDefaultCamera = true)
        {
            meshes = MeshesList.ToArray();
            sounds = SoundsList.ToArray();

            materials = MaterialsList.ToArray();
            multiMaterials = MultiMaterialsList.ToArray();
            shadowGenerators = ShadowGeneratorsList.ToArray();
            skeletons = SkeletonsList.ToArray();
            if (MorphTargetManagersList.Count > 0)
            {
                morphTargetManagers = MorphTargetManagersList.ToArray();
            }

            if (CamerasList.Count == 0 && generateDefaultCamera)
            {
                var camera = new BabylonCamera { name = "Default camera", id = Guid.NewGuid().ToString() };

                // Default camera init gives infinit values
                // Indeed, float.MaxValue - float.MinValue always leads to infinity
                var distanceVector = MaxVector - MinVector;
                var midPoint = MinVector + distanceVector / 2;
                camera.target = midPoint.ToArray();
                camera.position = (midPoint + distanceVector).ToArray();

                var distance = distanceVector.Length();
                camera.speed = distance / 50.0f;
                camera.maxZ = distance * 4f;

                camera.minZ = distance < 100.0f ? 0.1f : 1.0f;

                CamerasList.Add(camera);
            }

            if (LightsList.Count == 0 && generateDefaultLight)
            {
                var light = new BabylonLight
                {
                    name = "Default light",
                    id = Guid.NewGuid().ToString(),
                    type = 3,
                    groundColor = new float[] {0, 0, 0},
                    direction = new[] {0, 1.0f, 0},
                    intensity = 1
                };

                LightsList.Add(light);
            }

            cameras = (CamerasList.Count > 0) ? CamerasList.ToArray() : null;
            lights = (LightsList.Count > 0) ? LightsList.ToArray() : null;

            if (activeCameraID == null && CamerasList.Count > 0)
            {
                activeCameraID = CamerasList[0].id;
            }

            if(animationGroups != null && animationGroups.Count == 0)
            {
                animationGroups = null;
            }
        }

        internal bool TryPackIndexArrays()
        {
            bool result = true;

            foreach (var mesh in meshes)
            {
                result &= mesh.TryPackIndexArrays();
            }

            return result;
        }
    }
}
