using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;


[UseTemplate]
public class PixelWall : RootPanel
{
	static PixelWall Singleton;

	public Chat Chat { get; private set; }

	const float Offsetleft = 200;
	const float OffsetTop = 50;
	const float BoardSize = 950;
	const float Columns = 32;
	const float CellSize = BoardSize / Columns;
	const float HighlightBorder = 10.0f;
	const float CellSpacing = 0.3f;
	const float Padding = 2.0f;

	public Dictionary<Vector2, Color> CellColors = new();

	class TextEntry
	{
		public Vector2 Position;
		public Vector2 OriginalPosition;
		public string Text;
		public TimeSince TimeSinceCreated;
		public Color Color;
		public Rect Rect;
	}

	List<TextEntry> TextEntries = new();

	public PixelWall()
	{
		Singleton = this;
		AcceptsFocus = true;

		AddClass( "game-board" );

		Focus();
	}

	public void SetColor( string name, Color color, string displayname, string nameColor )
	{
		var parts = name.Split( "/", 2 );
		if ( parts == null || parts.Length != 2 )
			return;

		float x = parts[0].ToInt();
		float y = parts[1].ToInt();

		if ( x <= 0 || y <= 0 || x > Columns || y > Columns )
			return;

		var key = new Vector2( x, y );

		if ( CellColors.TryGetValue( key, out var currentColor ) && currentColor == color )
		{
			CellColors.Remove( key );
		}
		else
		{
			CellColors[key] = color;
		}

		TextEntries ??= new();

		x = Offsetleft + (x - 1) * CellSize;
		y = OffsetTop + (y - 1) * CellSize;

		var entry = new TextEntry();
		entry.OriginalPosition = new Vector2( x, y ) + new Vector2( CellSize * 0.9f, CellSize * 0.5f );
		entry.Position = new Vector2( x, y ) + new Vector2( CellSize * 2.5f, CellSize * 0.5f );

		entry.Text = displayname;
		entry.TimeSinceCreated = 0;
		entry.Color = Color.Parse( nameColor ) ?? Color.White;
		entry.Rect = new Rect( x - HighlightBorder, y - HighlightBorder, CellSize + HighlightBorder * 2, CellSize + HighlightBorder * 2 );

		TextEntries.Add( entry );

		if ( color == Color.Red )
		{
			PlaySound( "ui.button.over" );
		}
		else if ( color == Color.White )
		{
			PlaySound( "ui.popup.message.open" );
		}
		else if ( color == Color.Black )
		{
			PlaySound( "ui.popup.message.close" );
		}
		else if ( color == Color.Yellow )
		{
			PlaySound( "ui.navigate.deny" );
		}
		else if ( color == Color.Blue )
		{
			PlaySound( "ui.navigate.forward" );
		}
		else
		{
			PlaySound( "ui.button.press" );
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		// scale everything we draw to the UI scale
		var matrix = Matrix.CreateScale( ScaleToScreen ) * Matrix.CreateTranslation( Box.Rect.Position );
		using var matrixScope = Render.UI.MatrixScope( matrix );

		// Turn clip off
		Render.UI.Clip( null );

		var rect = new Rect( Offsetleft, OffsetTop, BoardSize, BoardSize );

		//
		// Draw the grid
		//
		for ( int x = 0; x < Columns; x++ )
		{
			for ( int y = 0; y < Columns; y++ )
			{
				Color color = new ColorHsv( ((x + y) * 20.0f + RealTime.Now * 100) % 360, 0.9f, 0.8f );
				var name = new Vector2( x + 1, y + 1 );

				if ( CellColors.TryGetValue( name, out var setColor ) )
				{
					color = setColor;
				}

				var cw = rect.width / Columns;
				var cell = new Rect( rect.left + x * cw + CellSpacing, rect.top + y * cw + CellSpacing, cw - CellSpacing, cw - CellSpacing );
				Render.UI.Box( cell, color );
			}
		}

		//
		// Cell Labels
		//
		for ( int x = 0; x < Columns; x++ )
		{
			Render.UI.Text( new Vector2( Offsetleft - 30, rect.top + x * CellSize + 4 ), Color.White.WithAlpha( 0.7f ), $"{x + 1}", "Poppins", 13, fontWeight: 600 );
			Render.UI.Text( new Vector2( rect.right + 5, rect.top + x * CellSize + 4 ), Color.White.WithAlpha( 0.7f ), $"{x + 1}", "Poppins", 13, fontWeight: 600 );

			Render.UI.Text( new Vector2( rect.left + x * CellSize + 5, 26 ), Color.White.WithAlpha( 0.7f ), $"{x + 1}", "Poppins", 13, fontWeight: 600 );
			Render.UI.Text( new Vector2( rect.left + x * CellSize + 5, rect.bottom + 5 ), Color.White.WithAlpha( 0.7f ), $"{x + 1}", "Poppins", 13, fontWeight: 600 );
		}

		//
		// Change names, effects
		//
		foreach ( var entry in TextEntries )
		{
			var highlight = entry.TimeSinceCreated.Relative.LerpInverse( 0.7f, 0.0f );
			if ( highlight > 0.0f )
			{
				highlight = Easing.EaseOut( highlight );

				Render.UI.BoxWithBorder( entry.Rect, Color.Transparent, HighlightBorder * highlight, Color.Random, corners: new Vector4( 2.0f ) );
			}

			var age = entry.TimeSinceCreated.Relative.LerpInverse( 5, 4f );

			for ( float i = 0; i < 1; i += 0.05f )
			{
				var pos = Vector3.Lerp( entry.OriginalPosition, entry.Position + Vector2.Down * -8.0f, i );

				var r = new Rect( pos.x, pos.y, 2, 2 );

				Render.UI.Box( r, Color.White.WithAlpha( age ), new Vector4( 10.0f ) );
			}

			Render.UI.Text( entry.Position + Vector2.One * -0.6f, Color.Black.WithAlpha( age ), entry.Text, "Poppins", 13, fontWeight: 800 );
			Render.UI.Text( entry.Position + Vector2.One * 0.6f, Color.Black.WithAlpha( age ), entry.Text, "Poppins", 13, fontWeight: 800 );
			Render.UI.Text( entry.Position, entry.Color.WithAlpha( age ), entry.Text, "Poppins", 13, fontWeight: 800 );

			var op = entry.OriginalPosition * 0.05f;

			var x = Noise.Perlin( op.x, op.y, RealTime.Now * 5.8f );
			var y = Noise.Perlin( op.y, op.x, RealTime.Now * 5.8f );

			entry.Position += new Vector2( x, y ) * Time.Delta * 100.0f;
		}

		TextEntries.RemoveAll( x => x.TimeSinceCreated > 5 );
	}

	internal bool OnStreamViewerMessage( StreamChatMessage message )
	{
		if ( !message.Message.StartsWith( "set " ) )
			return false;

		var text = message.Message[4..];

		var splits = text.Split( ' ', StringSplitOptions.RemoveEmptyEntries );
		if ( splits.Length <= 0 ) return false;

		//
		// set 2,2 red
		//
		if ( splits.Length == 2 )
		{
			var part = splits[0];
			var color = Color.Parse( splits[1] );
			if ( color.HasValue )
			{
				SetColor( part, color.Value, message.DisplayName, message.Color );
				return true;
			}
		}

		//
		// set 1 1 red
		//
		if ( splits.Length == 3 )
		{
			var part = splits[0].Trim( new[] { '/', ',' } ) + "/" + splits[1].Trim( new[] { '/', ',' } );
			var color = Color.Parse( splits[3] );
			if ( color.HasValue )
			{
				SetColor( part, color.Value, message.DisplayName, message.Color );
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Called when a message from a stream viewer comes in
	/// </summary>
	/// <param name="message"></param>
	[Event.Streamer.ChatMessage]
	public static void OnMessage( StreamChatMessage message )
	{
		if ( Singleton?.OnStreamViewerMessage( message ) ?? false )
			return;

		Singleton?.Chat?.AddEntry( message.DisplayName, message.Message, null, message.Color );
	}
}
