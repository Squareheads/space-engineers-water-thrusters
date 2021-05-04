using Jakaria;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using VRage.Game;
//using Sandbox.Game.Multiplayer
using VRage.Game.ModAPI;

namespace Integrity
{
    public sealed class SharedDataSingleton
    {
        internal Dictionary<MyPlanet, Water> WaterMap = new Dictionary<MyPlanet, Water>();
        internal Dictionary<MyPlanet, double> MaxWaterHeightSqr = new Dictionary<MyPlanet, double>();

        private SharedDataSingleton()
        {
        }

        public static SharedDataSingleton Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly SharedDataSingleton instance = new SharedDataSingleton();
        }
    }



    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation, int.MaxValue - 1)]
    public partial class Session : MySessionComponentBase
    {
        internal readonly WaterModAPI WApi = new WaterModAPI();
        internal readonly Dictionary<MyPlanet, Water> WaterMap = new Dictionary<MyPlanet, Water>();
        internal readonly Dictionary<MyPlanet, double> MaxWaterHeightSqr = new Dictionary<MyPlanet, double>();

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            WApi.Register("WaterThrusters");
            WApi.RecievedData += WApiReceiveData;
        }

        protected override void UnloadData()
        {
            WApi.Unregister();
            WApi.RecievedData -= WApiReceiveData;
        }

		private void WApiReceiveData()
        {
            if (WApi.Registered)
            {
                WaterMap.Clear();
                MaxWaterHeightSqr.Clear();
                for (int i = 0; i < WApi.Waters.Count; i++)
                {

                    var water = WApi.Waters[i];
                    if (water.planet != null)
                    {

                        WaterMap[water.planet] = water;
                        var maxWaterHeight = water.radius;
                        var maxWaterHeightSqr = maxWaterHeight * maxWaterHeight;
                        MaxWaterHeightSqr[water.planet] = maxWaterHeightSqr;

                        SharedDataSingleton.Instance.WaterMap[water.planet] = water;
                        SharedDataSingleton.Instance.MaxWaterHeightSqr[water.planet] = maxWaterHeightSqr;
                    }
                }
            }
        }

    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Thrust), false)]
    public class WaterThrusterComponent : MyGameLogicComponent
    {
        public MyThrust Block;
        WaterModAPI api;
		

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Block = (MyThrust)Entity;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            if (!Block.IsFunctional){return;}

            var thrust = Block as Sandbox.ModAPI.Ingame.IMyThrust;
			if (!thrust.Enabled){return;}
			
            var waters = SharedDataSingleton.Instance.WaterMap;
            if (waters == null){return;}
			
			// NPCs can use every thruster under water
			IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(Block.OwnerId);
            if (faction != null)
            {
                // this does work for a player not being in any faction and SPRT Pirates at least
                if (faction.IsEveryoneNpc()) { return; };
            }

            var isUnderwater = false;
            foreach (var item in waters)
            {
                if (!item.Value.IsUnderwater(Block.PositionComp.GetPosition()))
                    continue;

                isUnderwater = true;
            }

            string ThrusterName = thrust.DefinitionDisplayNameText;
            if (ThrusterName.Contains("Water"))
            {
				if (isUnderwater == false){thrust.Enabled = false;}
            }
            else
            {
                if (isUnderwater == true){thrust.Enabled = false;}
            }
        }

    }
}