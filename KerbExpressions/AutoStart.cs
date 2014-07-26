using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    [KSPAddonImproved(KSPAddonImproved.Startup.MainMenu, false)]
    public class AutoStart : MonoBehaviour
    {
        public string SaveFolder = "sandbox";
        public string VesselName = "Jebediah Kerman";

        private static bool _firstStart = true;

        public void Start()
        {
            if (_firstStart)
            {
                _firstStart = false;
                LoadGame();
            }
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                LoadGame();
            }
        }

        private void LoadGame()
        {
            Util.Log("Attempting to auto-load " + SaveFolder + " vessel " + VesselName);
            var game = GamePersistence.LoadGame("persistent", SaveFolder, true, false);
            if (game != null)
            {
                game.startScene = GameScenes.FLIGHT;

                var protoVessel = game.flightState.protoVessels.Where((p) => p.vesselName == VesselName).FirstOrDefault();
                if (protoVessel != null)
                {
                    game.flightState.activeVesselIdx = game.flightState.protoVessels.IndexOf(protoVessel);
                }
                else
                {
                    game.flightState.activeVesselIdx = 0;
                }
                HighLogic.CurrentGame = game;
                HighLogic.SaveFolder = SaveFolder;
                Util.Log("Starting game...");
                HighLogic.CurrentGame.Start();
            }
            else
            {
                Util.Log("Cannot load save game: " + SaveFolder);
            }
        }
    }
}
