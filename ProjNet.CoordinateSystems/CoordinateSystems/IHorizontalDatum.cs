// Copyright 2005 - 2009 - Morten Nielsen (www.sharpgis.net)
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

namespace ProjNet.CoordinateSystems
{
	/// <summary>
	/// Procedure used to measure positions on the surface of the Earth.
	/// </summary>
	public interface IHorizontalDatum : IDatum
	{
		/// <summary>
		/// Gets or sets the ellipsoid of the datum.
		/// </summary>
		IEllipsoid Ellipsoid { get; set; }
		/// <summary>
		/// Gets preferred parameters for a Bursa Wolf transformation into WGS84. The 7 returned values 
		/// correspond to (dx,dy,dz) in meters, (ex,ey,ez) in arc-seconds, and scaling in parts-per-million.
		/// </summary>
		Wgs84ConversionInfo Wgs84Parameters { get; set; }
	}
}
