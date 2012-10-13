﻿using System;
using Nexus;
using Rasterizr.Studio.Modules.GraphicsDebugging.ViewModels;
using Rasterizr.Studio.Modules.TracefileViewer;

namespace Rasterizr.Studio.Modules.GraphicsDebugging
{
	public interface ISelectionService
	{
		event EventHandler<TracefileFrameChangedEventArgs> SelectedFrameChanged;
		event EventHandler<TracefileEventChangedEventArgs> SelectedEventChanged;
		event EventHandler<PixelChangedEventArgs> SelectedPixelChanged;

		TracefileFrameViewModel SelectedFrame { get; set; }
		TracefileEventViewModel SelectedEvent { get; set; }
		IntPoint2D SelectedPixel { get; set; }
	}
}