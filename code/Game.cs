using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TwitchLab
{
	public partial class Game : Sandbox.Game
	{
		public PixelWall Hud { get; private set; }

		private readonly Dictionary<string, Player> Players = new();

		public Game()
		{
			if ( IsClient )
			{
				Hud = new PixelWall();
				Local.Hud = Hud;
			}
		}

		public override void Shutdown()
		{

		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( Local.Hud == Hud )
			{
				Local.Hud = null;
			}

			Hud?.Delete();
			Hud = null;
		}

		Angles angles;

		public override CameraSetup BuildCamera( CameraSetup camSetup )
		{
			angles += new Angles( 0, 1.0f, 0.0f ) * RealTime.Delta;

			camSetup.Rotation = Rotation.From( angles );
			camSetup.Position = new Vector3( 0, 0, 130 + MathF.Sin( RealTime.Now * 0.2f ) * 100 );
			camSetup.FieldOfView = 80;
			camSetup.Ortho = false;
			camSetup.Viewer = null;

			return camSetup;
		}
	}
}
