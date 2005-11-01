//============================================================================
//ZedGraph Class Library - A Flexible Line Graph/Bar Graph Library in C#
//Copyright (C) 2005  John Champion
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//Lesser General Public License for more details.
//
//You should have received a copy of the GNU Lesser General Public
//License along with this library; if not, write to the Free Software
//Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

#region Using directives

using System;
using System.Collections;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;

#endregion

namespace ZedGraph
{
	/// <summary>
	/// A collection class containing a list of <see cref="Y2Axis"/> objects.
	/// </summary>
	/// 
	/// <author>John Champion</author>
	/// <version> $Revision: 3.1 $ $Date: 2005-09-24 09:13:32 $ </version>
	[Serializable]
	public class Y2AxisList : AxisList, ICloneable
	{

	#region Constructors

		/// <summary>
		/// Default constructor for the collection class.
		/// </summary>
		public Y2AxisList()
		{
		}

		/// <summary>
		/// The Copy Constructor
		/// </summary>
		/// <param name="rhs">The <see cref="Y2AxisList"/> object from which to copy</param>
		public Y2AxisList( Y2AxisList rhs )
		{
			foreach ( Y2Axis item in rhs )
			{
				this.Add( new Y2Axis( item ) );
			}
		}
				
		/// <summary>
		/// Deep-copy clone routine
		/// </summary>
		/// <returns>A new, independent copy of the <see cref="Y2AxisList"/>.</returns>
		public object Clone()
		{ 
			return new Y2AxisList( this ); 
		}
		
	#endregion

	#region List Methods

		/// <summary>
		/// Add a <see cref="Y2Axis"/> object to the collection at the end of the list.
		/// </summary>
		/// <param name="axis">A reference to the <see cref="Y2Axis"/> object to
		/// be added</param>
		/// <seealso cref="IList.Add"/>
		public int Add( Y2Axis axis )
		{
			return List.Add( axis );
		}

		/// <summary>
		/// Insert a <see cref="Y2Axis"/> object into the collection at the specified
		/// zero-based index location.
		/// </summary>
		/// <param name="index">The zero-based index location for insertion.</param>
		/// <param name="axis">A reference to the <see cref="Y2Axis"/> object that is to be
		/// inserted.</param>
		/// <seealso cref="IList.Insert"/>
		public void Insert( int index, Y2Axis axis )
		{
			List.Insert( index, axis );
		}

	#endregion

	}
}