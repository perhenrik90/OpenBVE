using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.Stations;

namespace CsvRwRouteParser {
	internal partial class Parser {
		internal string ObjectPath;
		internal string SoundPath;
		internal string TrainPath;
		internal string CompatibilityFolder;
		internal bool CylinderHack = false;
		internal bool IsRW;

		internal Plugin Plugin;

		internal CurrentRoute CurrentRoute;
		// parse route
		internal void ParseRoute(string FileName, bool isRW, System.Text.Encoding Encoding, string trainPath, string objectPath, string soundPath, string compatibilitySignalSet, bool PreviewOnly, Plugin hostPlugin)
		{
			Plugin = hostPlugin;
			CurrentRoute = Plugin.CurrentRoute;
			/*
			 * Store paths for later use
			 */
			ObjectPath = objectPath;
			SoundPath = soundPath;
			TrainPath = trainPath;
			IsRW = isRW;
			if (!PreviewOnly)
			{
				for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
				{
					if (Plugin.CurrentHost.Plugins[i].Object != null)
					{
						Plugin.CurrentHost.Plugins[i].Object.SetObjectParser(SoundPath); //HACK: Pass out the sound folder path to those plugins which consume it
					}
				}
			}
			freeObjCount = 0;
			railtypeCount = 0;
			Plugin.CurrentOptions.UnitOfSpeed = "km/h";
			Plugin.CurrentOptions.SpeedConversionFactor = 0.0;
			CompatibilityFolder = Plugin.FileSystem.GetDataFolder("Compatibility");
			if (!PreviewOnly)
			{
				CompatibilityObjects.LoadCompatibilityObjects(Path.CombineFile(CompatibilityFolder,"CompatibilityObjects.xml"));
			}
			RouteData Data = new RouteData
			{
				BlockInterval = 25.0,
				AccurateObjectDisposal = false,
				FirstUsedBlock = -1,
				Blocks = new Block[1]
			};
			Data.Blocks[0] = new Block(PreviewOnly);
			Data.Blocks[0].Rails.Add(0, new Rail { RailStarted =  true });
			Data.Blocks[0].RailType = new[] { 0 };
			Data.Blocks[0].Accuracy = 2.0;
			Data.Blocks[0].AdhesionMultiplier = 1.0;
			Data.Blocks[0].CurrentTrackState = new TrackElement(0.0);
			if (!PreviewOnly)
			{
				Data.Blocks[0].Background = 0;
				Data.Blocks[0].Fog = new Fog(CurrentRoute.NoFogStart, CurrentRoute.NoFogEnd, Color24.Grey, 0);
				Data.Blocks[0].Cycle = new[] {-1};
				Data.Blocks[0].RailCycles = new RailCycle[1];
				Data.Blocks[0].RailCycles[0].RailCycleIndex = -1;
				Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
				Data.Blocks[0].RailFreeObj = new FreeObj[][] {};
				Data.Blocks[0].GroundFreeObj = new FreeObj[] {};
				Data.Blocks[0].RailWall = new WallDike[] {};
				Data.Blocks[0].RailDike = new WallDike[] {};
				Data.Blocks[0].RailPole = new Pole[] {};
				Data.Markers = new Marker[] {};
				Data.RequestStops = new StopRequest[] { };
				string PoleFolder = Path.CombineDirectory(CompatibilityFolder, "Poles");
				Data.Structure.Poles = new PoleDictionary();
				Data.Structure.Poles.Add(0, new ObjectDictionary());
				Data.Structure.Poles[0].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_1.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles.Add(1, new ObjectDictionary());
				Data.Structure.Poles[1].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_2.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles.Add(2, new ObjectDictionary());
				Data.Structure.Poles[2].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_3.csv"), System.Text.Encoding.UTF8, false));
				Data.Structure.Poles.Add(3, new ObjectDictionary());
				Data.Structure.Poles[3].Add(0, LoadStaticObject(Path.CombineFile(PoleFolder, "pole_4.csv"), System.Text.Encoding.UTF8, false));
				
				Data.Structure.RailObjects = new ObjectDictionary();
				Data.Structure.RailObjects = new ObjectDictionary();
				Data.Structure.Ground = new ObjectDictionary();
				Data.Structure.WallL = new ObjectDictionary();
				Data.Structure.WallR = new ObjectDictionary();
				Data.Structure.DikeL = new ObjectDictionary();
				Data.Structure.DikeR = new ObjectDictionary();
				Data.Structure.FormL = new ObjectDictionary();
				Data.Structure.FormR = new ObjectDictionary();
				Data.Structure.FormCL = new ObjectDictionary();
				Data.Structure.FormCR = new ObjectDictionary();
				Data.Structure.RoofL = new ObjectDictionary();
				Data.Structure.RoofR = new ObjectDictionary();
				Data.Structure.RoofCL = new ObjectDictionary();
				Data.Structure.RoofCR = new ObjectDictionary();
				Data.Structure.CrackL = new ObjectDictionary();
				Data.Structure.CrackR = new ObjectDictionary();
				Data.Structure.FreeObjects = new ObjectDictionary();
				Data.Structure.Beacon = new ObjectDictionary();
				Data.Structure.Cycles = new int[][] {};
				Data.Structure.RailCycles = new int[][] { };
				Data.Structure.Run = new int[] {};
				Data.Structure.Flange = new int[] {};
				Data.Backgrounds = new BackgroundDictionary();
				Data.TimetableDaytime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				Data.TimetableNighttime = new OpenBveApi.Textures.Texture[] {null, null, null, null};
				// signals
				Data.Signals = new SignalDictionary();
				if (compatibilitySignalSet == null) //not selected via main form
				{
					compatibilitySignalSet = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility"), "Signals\\Japanese.xml");
				}
				CompatibilitySignalObject.ReadCompatibilitySignalXML(Plugin.CurrentHost, compatibilitySignalSet, out Data.CompatibilitySignals, out CompatibilityObjects.SignalPost, out Data.SignalSpeeds);
				// game data
				CurrentRoute.Sections = new[]
				{
					new RouteManager2.SignalManager.Section(0, new[] { new SectionAspect(0, 0.0), new SectionAspect(4, double.PositiveInfinity) }, SectionType.IndexBased)
				};
				
				CurrentRoute.Sections[0].CurrentAspect = 0;
				CurrentRoute.Sections[0].StationIndex = -1;
			}
			ParseRouteForData(FileName, Encoding, ref Data, PreviewOnly);
			if (Plugin.Cancel)
			{
				Plugin.IsLoading = false;
				return;
			}
			ApplyRouteData(FileName, ref Data, PreviewOnly);
		}

		private void ParseRouteForData(string FileName, System.Text.Encoding Encoding, ref RouteData Data, bool PreviewOnly) {
			//Read the entire routefile into memory
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			Expression[] Expressions;
			PreprocessSplitIntoExpressions(FileName, Lines, out Expressions, true);
			PreprocessChrRndSub(FileName, Encoding, ref Expressions);
			double[] UnitOfLength = new double[] { 1.0 };
			//Set units of speed initially to km/h
			//This represents 1km/h in m/s
			Data.UnitOfSpeed = 0.277777777777778;
			PreprocessOptions(Expressions, ref Data, ref UnitOfLength, PreviewOnly);
			PreprocessSortByTrackPosition(UnitOfLength, ref Expressions);
			ParseRouteForData(FileName, Encoding, Expressions, UnitOfLength, ref Data, PreviewOnly);
			CurrentRoute.UnitOfLength = UnitOfLength;
		}
		
		private int freeObjCount;
		private int railtypeCount;
		private readonly System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

		// parse route for data
		private void ParseRouteForData(string FileName, System.Text.Encoding Encoding, Expression[] Expressions, double[] UnitOfLength, ref RouteData Data, bool PreviewOnly) {
			CurrentStation = -1;
			CurrentStop = -1;
			CurrentSection = 0;
			
			string Section = ""; bool SectionAlwaysPrefix = false;
			int BlockIndex = 0;
			int BlocksUsed = Data.Blocks.Length;
			CurrentRoute.Stations = new RouteStation[] { };
			Data.RequestStops = new StopRequest[] { };
			double progressFactor = Expressions.Length == 0 ? 0.3333 : 0.3333 / Expressions.Length;
			// process non-track namespaces
			//Check for any special-cased fixes we might need
			CheckRouteSpecificFixes(FileName, ref Data, ref Expressions, PreviewOnly);
			//Apply parameters to object loaders
			if (!PreviewOnly)
			{
				for (int i = 0; i < Plugin.CurrentHost.Plugins.Length; i++)
				{
					if (Plugin.CurrentHost.Plugins[i].Object != null)
					{
						Plugin.CurrentHost.Plugins[i].Object.SetCompatibilityHacks(Plugin.CurrentOptions.EnableBveTsHacks, CylinderHack);
						//Remember that these will be ignored if not the correct plugin
						Plugin.CurrentHost.Plugins[i].Object.SetObjectParser(Plugin.CurrentOptions.CurrentXParser);
						Plugin.CurrentHost.Plugins[i].Object.SetObjectParser(Plugin.CurrentOptions.CurrentObjParser);
					}
				}
			}
			
			for (int j = 0; j < Expressions.Length; j++) {
				Plugin.CurrentProgress = j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel)
					{
						Plugin.IsLoading = false;
						return;
					}
				}
				if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim(new char[] { });
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					// find equals
					if (IsRW)
					{
						Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					}
					
					// separate command and arguments
					string Command, ArgumentSequence;
					Expressions[j].SeparateCommandsAndArguments(out Command, out ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.IsValidDouble(Command, UnitOfLength)) {
						// track position (ignored)
					} else {
						string[] Arguments = SplitArguments(ArgumentSequence);

						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							if (Arguments.Length >= 1) {
								Section = Arguments[0];
								SectionAlwaysPrefix = false;
							} else {
								Section = "";
								SectionAlwaysPrefix = false;
							}
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
							if (Command.StartsWith("structure", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd(new char[] { });
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd(new char[] { });
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.background.x" + Command.Substring(18, Command.Length - 20).TrimEnd(new char[] { });
							} else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.background.aspect" + Command.Substring(18, Command.Length - 25).TrimEnd(new char[] { });
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.background.x" + Command.Substring(14, Command.Length - 16).TrimEnd(new char[] { });
							} else if (Command.StartsWith("structure.back", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase))
							{
								Command = "texture.background.aspect" + Command.Substring(14, Command.Length - 21).TrimEnd(new char[] { });
							} else if (Command.StartsWith("cycle", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".params", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 7).TrimEnd(new char[] { });
							} else if (Command.StartsWith("signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 5).TrimEnd(new char[] { });
							} else if (Command.StartsWith("train.run", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 4).TrimEnd(new char[] { });
							} else if (Command.StartsWith("train.flange", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))
							{
								Command = Command.Substring(0, Command.Length - 4).TrimEnd(new char[] { });
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 24).Trim(new char[] { });
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night.load", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 26).Trim(new char[] { });
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.day" + Command.Substring(15, Command.Length - 19).Trim(new char[] { });
							} else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.timetable.night" + Command.Substring(15, Command.Length - 21).Trim(new char[] { });
							} else if (Command.StartsWith("route.signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase))  {
								Command = Command.Substring(0, Command.Length - 4).TrimEnd(new char[] { });
							} else if (Command.StartsWith("route.runinterval", StringComparison.OrdinalIgnoreCase)) {
								Command = "train.interval" + Command.Substring(17, Command.Length - 17);
							} else if (Command.StartsWith("train.gauge", StringComparison.OrdinalIgnoreCase)) {
								Command = "route.gauge" + Command.Substring(11, Command.Length - 11);
							} else if (Command.StartsWith("texture.", StringComparison.OrdinalIgnoreCase)) {
								Command = "structure." + Command.Substring(8, Command.Length - 8);
							}
						}

						int[] commandIndices = FindIndices(ref Command, Expressions[j]);

						// process command
						if (!string.IsNullOrEmpty(Command))
						{
							int period = Command.IndexOf('.');
							string nameSpace = string.Empty;
							if (period != -1)
							{
								nameSpace = Command.Substring(0, period).ToLowerInvariant();
								Command = Command.Substring(period + 1);
							}
							Command = Command.ToLowerInvariant();
							
							switch (nameSpace)
							{
								case "options":
									ParseOptionCommand(Command, Arguments, UnitOfLength, Expressions[j], ref Data, PreviewOnly);
									break;
								case "route":
									ParseRouteCommand(Command, Arguments, commandIndices[0], FileName, UnitOfLength, Expressions[j], ref Data, PreviewOnly);
									break;
								case "train":
									ParseTrainCommand(Command, Arguments, commandIndices[0], Expressions[j], ref Data, PreviewOnly);
									break;
								case "structure":
								case "texture":
									ParseStructureCommand(Command, Arguments, commandIndices, Encoding, Expressions[j], ref Data, PreviewOnly);
									break;
								case "":
									ParseSignalCommand(Command, Arguments, commandIndices[0], Encoding, Expressions[j], ref Data, PreviewOnly);
									break;
								case "cycle":
									ParseCycleCommand(Command, Arguments, commandIndices[0], Expressions[j], ref Data, PreviewOnly);
									break;
								case "track":
									break;
								// ReSharper disable once RedundantEmptySwitchSection
								default:
									/*
									 * This needs an unrecognised command at some stage
									 */
									break;
							}
						}
					}
				}
			}
			
			// process track namespace
			for (int j = 0; j < Expressions.Length; j++) {
				Plugin.CurrentProgress = 0.3333 + j * progressFactor;
				if ((j & 255) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Plugin.Cancel)
					{
						Plugin.IsLoading = false;
						return;
					}
				}
				if (Data.LineEndingFix)
				{
					if (Expressions[j].Text.EndsWith("_"))
					{
						Expressions[j].Text = Expressions[j].Text.Substring(0, Expressions[j].Text.Length - 1).Trim(new char[] { });
					}
				}
				if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
					Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim(new char[] { });
					if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Structure";
					} else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
						Section = "Track";
					}
					SectionAlwaysPrefix = true;
				} else {
					if (IsRW)
					{
						Expressions[j].ConvertRwToCsv(Section, SectionAlwaysPrefix);
					}
					// separate command and arguments
					string Command, ArgumentSequence;
					Expressions[j].SeparateCommandsAndArguments(out Command, out ArgumentSequence, Culture, false, IsRW, Section);
					// process command
					double currentTrackPosition;
					bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
					if (NumberCheck && NumberFormats.TryParseDouble(Command, UnitOfLength, out currentTrackPosition)) {
						// track position
						if (ArgumentSequence.Length != 0) {
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "A track position must not contain any arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						} else if (currentTrackPosition < 0.0) {
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Negative track position encountered at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
						} else {
							if (Plugin.CurrentOptions.EnableBveTsHacks && IsRW && currentTrackPosition == 4535545100)
							{
								//WMATA Red line has an erroneous track position causing an out of memory cascade
								currentTrackPosition = 45355;
							}
							Data.TrackPosition = currentTrackPosition;
							BlockIndex = (int)Math.Floor(currentTrackPosition / Data.BlockInterval + 0.001);
							if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
							Data.CreateMissingBlocks(ref BlocksUsed, BlockIndex, PreviewOnly);
						}
					} else {
						string[] Arguments = SplitArguments(ArgumentSequence);
						
						// preprocess command
						if (Command.ToLowerInvariant() == "with") {
							if (Arguments.Length >= 1) {
								Section = Arguments[0];
								SectionAlwaysPrefix = false;
							} else {
								Section = "";
								SectionAlwaysPrefix = false;
							}
							Command = null;
						} else {
							if (Command.StartsWith(".")) {
								Command = Section + Command;
							} else if (SectionAlwaysPrefix) {
								Command = Section + "." + Command;
							}
							Command = Command.Replace(".Void", "");
						}
						
						// process command
						if (!string.IsNullOrEmpty(Command)) {
							int period = Command.IndexOf('.');
							string nameSpace = string.Empty;
							if (period != -1)
							{
								nameSpace = Command.Substring(0, period).ToLowerInvariant();
								Command = Command.Substring(period + 1);
							}
							if (nameSpace.StartsWith("signal", StringComparison.InvariantCultureIgnoreCase))
							{
								nameSpace = "";
							}
							Command = Command.ToLowerInvariant();

							switch (nameSpace)
							{
								case "track":
									ParseTrackCommand(Command, Arguments, FileName, UnitOfLength, Expressions[j], ref Data, BlockIndex, PreviewOnly);
									break;
								case "options":
								case "route":
								case "train":
								case "structure":
								case "texture":
								case "":
								case "cycle":
									break;
								default:
									Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "The command " + Command + " is not supported at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + Expressions[j].File);
									break;
							}
							
						}
					}
				}
			}
			// blocks
			Array.Resize(ref Data.Blocks, BlocksUsed);
		}
	}
}
