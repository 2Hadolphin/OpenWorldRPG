using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Return
{
    /// <summary>
    /// Singleton game installer.
    /// </summary>
    public class GameInstaller : SceneInstaller
    {
        #region Singleton

        static DiContainer ps_container;

        public static void SetSceneInstaller(DiContainer container)
        {
            Assert.IsTrue(ps_container != container, $"Scene container should be as singleton.\n {container} conflict with {ps_container}.");
            ps_container = container;

        }

        public static void Inject(object obj)
        {
            if (obj is GameObject go)
                ps_container.InjectGameObject(go);
            else
                ps_container.Inject(obj);
        }

        #endregion

        public override void InstallBindings()
        {
            base.InstallBindings();

            ps_container = Container;
        }


    }
}