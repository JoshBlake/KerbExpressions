using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    [KSPAddonImproved(KSPAddonImproved.Startup.Flight, false)]
    public class KerbExpressionBehavior : MonoBehaviour
    {
        private GameScenes _lastScene = GameScenes.LOADING;

        ExpressionController _expController;

        BodySourceManager _bodySourceManager;

        public KerbExpressionBehavior()
        {
            Util.Log("Constructed.");
        }

        public void Awake()
        {
            try
            {
                var gameObject = new GameObject();
                _bodySourceManager = gameObject.AddComponent<BodySourceManager>();

                _expController = new ExpressionController(_bodySourceManager);


                Util.Log("Awake.");
            }
            catch (Exception ex)
            {
                Util.Log("{0}: {1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
            }
        }

        public void Start()
        {
            try
            {
                _expController.Start();
            }
            catch (Exception ex)
            {
                Util.Log("{0}: {1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
            }
        }

        public void LateUpdate()
        {
            try
            {
                if (HighLogic.LoadedScene != _lastScene)
                {
                    _lastScene = HighLogic.LoadedScene;
                    Util.Log("Saw scene switch to {0}", _lastScene.ToString());
                }

                _expController.Update();
            }
            catch (Exception ex)
            {
                Util.Log("{0}: {1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
            }
        }

        public void OnDestroy()
        {
            Util.Log("Destroyed");
        }
    }
}
