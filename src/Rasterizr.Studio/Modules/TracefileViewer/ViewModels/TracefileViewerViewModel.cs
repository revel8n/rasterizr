﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using Gemini.Framework;
using Rasterizr.Diagnostics.Logging;
using Rasterizr.Studio.Modules.GraphicsDebugging;
using Rasterizr.Studio.Modules.GraphicsDebugging.ViewModels;

namespace Rasterizr.Studio.Modules.TracefileViewer.ViewModels
{
	public class TracefileViewerViewModel : Document
	{
		private readonly string _fileName;
		private readonly TracefileViewModel _tracefile;

		[Import]
		private ISelectionService _selectionService;

		public override string DisplayName
		{
			get { return Path.GetFileName(_fileName); }
		}

		public IList<TracefileFrameViewModel> Frames
		{
			get { return _tracefile.Frames; }
		}

		private TracefileFrameViewModel _selectedFrame;
		public TracefileFrameViewModel SelectedFrame
		{
			get { return _selectedFrame; }
			set
			{
				_selectedFrame = value;
				_selectionService.SelectedFrame = value;
				NotifyOfPropertyChange(() => SelectedFrame);
			}
		}

		public TracefileViewerViewModel(string fileName)
		{
			_fileName = fileName;
			using (var fileStream = File.OpenRead(fileName))
				_tracefile = new TracefileViewModel(Tracefile.FromTextReader(new StreamReader(fileStream)));
		}
	}
}