using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class BuildScript
    {
        [MenuItem("Tools/CI/Build Windows")]
        public static void BuildWindows()
        {
            string[] scenes = { "Assets/_Project/Scenes/00_Bootstrap.unity", "Assets/_Project/Scenes/SampleScene.unity" };
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/CRPG_Windows.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build completed successfully!");
        }
        
        [MenuItem("Tools/CI/Build Linux")]
        public static void BuildLinux()
        {
            string[] scenes = { "Assets/_Project/Scenes/00_Bootstrap.unity", "Assets/_Project/Scenes/SampleScene.unity" };
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Build/CRPG_Linux",
                target = BuildTarget.StandaloneLinux64,
                options = BuildOptions.None
            };
            
            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build completed successfully!");
        }
    }
}
