﻿using OpenBveApi.Math;
using OpenBveApi.World;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	internal class Signal
	{
		internal Signal(double trackPosition, int sectionIndex, SignalObject signalObject, Vector2 position, double yaw, double pitch, double roll, bool showObject, bool showPost)
		{
			TrackPosition = trackPosition;
			SectionIndex = sectionIndex;
			SignalObject = signalObject;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			ShowObject = showObject;
			ShowPost = showPost;
		}

		internal void Create(Vector3 wpos, Transformation RailTransformation, double StartingDistance, double EndingDistance, double Brightness)
		{
			double dz = TrackPosition - StartingDistance;
			if (ShowPost)
			{
				// post
				double dx = Position.X;
				wpos += dx * RailTransformation.X + dz * RailTransformation.Z;
				CompatibilityObjects.SignalPost.CreateObject(wpos, RailTransformation, Transformation.NullTransformation, -1, StartingDistance, EndingDistance, TrackPosition, Brightness);
			}
			if (ShowObject)
			{
				// signal object
				double dx = Position.X;
				double dy = Position.Y;
				wpos += dx * RailTransformation.X + dy * RailTransformation.Y + dz * RailTransformation.Z;
				SignalObject.Create(wpos, RailTransformation, new Transformation(Yaw, Pitch, Roll), SectionIndex, StartingDistance, EndingDistance, TrackPosition, Brightness);
			}
		}

		internal readonly double TrackPosition;
		private readonly int SectionIndex;
		private readonly SignalObject SignalObject;
		private readonly Vector2 Position;
		private readonly double Yaw;
		internal readonly double Pitch;
		private readonly double Roll;
		private readonly bool ShowObject;
		private readonly bool ShowPost;
	}
}
