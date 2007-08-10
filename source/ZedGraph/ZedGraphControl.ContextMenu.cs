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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;

namespace ZedGraph
{
	partial class ZedGraphControl
	{

	#region ContextMenu

		// Revision: JCarpenter 10/06
		/// <summary>
		/// Public enumeration that specifies the type of 
		/// object present at the Context Menu's mouse location
		/// </summary>
		public enum ContextMenuObjectState
		{
			/// <summary>
			/// The object is an Inactive Curve Item at the Context Menu's mouse position
			/// </summary>
			InactiveSelection,
			/// <summary>
			/// The object is an active Curve Item at the Context Menu's mouse position
			/// </summary>
			ActiveSelection,
			/// <summary>
			/// There is no selectable object present at the Context Menu's mouse position
			/// </summary>
			Background
		}

		//Revision: JCarpenter 10/06
		/// <summary>
		/// Find the object currently under the mouse cursor, and return its state.
		/// </summary>
		private ContextMenuObjectState GetObjectState()
		{
			ContextMenuObjectState objState = ContextMenuObjectState.Background;

			// Determine object state
			Point mousePt = this.PointToClient( Control.MousePosition );
			int iPt;
			GraphPane pane;
			object nearestObj;

			using ( Graphics g = this.CreateGraphics() )
			{
				if ( this.MasterPane.FindNearestPaneObject( mousePt, g, out pane,
						out nearestObj, out iPt ) )
				{
					CurveItem item = nearestObj as CurveItem;

					if ( item != null && iPt >= 0 )
					{
						if ( item.IsSelected )
							objState = ContextMenuObjectState.ActiveSelection;
						else
							objState = ContextMenuObjectState.InactiveSelection;
					}
				}
			}

			return objState;
		}

		/// <summary>
		/// protected method to handle the popup context menu in the <see cref="ZedGraphControl"/>.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void contextMenuStrip1_Opening( object sender, CancelEventArgs e )
		{
			// disable context menu by default
			e.Cancel = true;
			ContextMenuStrip menuStrip = sender as ContextMenuStrip;

			//Revision: JCarpenter 10/06
			ContextMenuObjectState objState = GetObjectState();

			if ( _masterPane != null && menuStrip != null )
			{
				menuStrip.Items.Clear();

				_isZooming = false;
				_isPanning = false;
				Cursor.Current = Cursors.Default;

				_menuClickPt = this.PointToClient( Control.MousePosition );
				GraphPane pane = _masterPane.FindPane( _menuClickPt );

				if ( _isShowContextMenu )
				{
					string menuStr = string.Empty;

					ToolStripMenuItem item = new ToolStripMenuItem();
					item.Name = "copy";
					item.Tag = "copy";
					item.Text = _resourceManager.GetString( "copy" );
					item.Click += new System.EventHandler( this.MenuClick_Copy );
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "save_as";
					item.Tag = "save_as";
					item.Text = _resourceManager.GetString( "save_as" );
					item.Click += new System.EventHandler( this.MenuClick_SaveAs );
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "page_setup";
					item.Tag = "page_setup";
					item.Text = _resourceManager.GetString( "page_setup" );
					item.Click += new System.EventHandler( this.MenuClick_PageSetup );
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "print";
					item.Tag = "print";
					item.Text = _resourceManager.GetString( "print" );
					item.Click += new System.EventHandler( this.MenuClick_Print );
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "show_val";
					item.Tag = "show_val";
					item.Text = _resourceManager.GetString( "show_val" );
					item.Click += new System.EventHandler( this.MenuClick_ShowValues );
					item.Checked = this.IsShowPointValues;
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "unzoom";
					item.Tag = "unzoom";

					if ( pane == null || pane.ZoomStack.IsEmpty )
						menuStr = _resourceManager.GetString( "unzoom" );
					else
					{
						switch ( pane.ZoomStack.Top.Type )
						{
							case ZoomState.StateType.Zoom:
							case ZoomState.StateType.WheelZoom:
								menuStr = _resourceManager.GetString( "unzoom" );
								break;
							case ZoomState.StateType.Pan:
								menuStr = _resourceManager.GetString( "unpan" );
								break;
							case ZoomState.StateType.Scroll:
								menuStr = _resourceManager.GetString( "unscroll" );
								break;
						}
					}

					//menuItem.Text = "Un-" + ( ( pane == null || pane.zoomStack.IsEmpty ) ?
					//	"Zoom" : pane.zoomStack.Top.TypeString );
					item.Text = menuStr;
					item.Click += new EventHandler( this.MenuClick_ZoomOut );
					if ( pane == null || pane.ZoomStack.IsEmpty )
						item.Enabled = false;
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "undo_all";
					item.Tag = "undo_all";
					menuStr = _resourceManager.GetString( "undo_all" );
					item.Text = menuStr;
					item.Click += new EventHandler( this.MenuClick_ZoomOutAll );
					if ( pane == null || pane.ZoomStack.IsEmpty )
						item.Enabled = false;
					menuStrip.Items.Add( item );

					item = new ToolStripMenuItem();
					item.Name = "set_default";
					item.Tag = "set_default";
					menuStr = _resourceManager.GetString( "set_default" );
					item.Text = menuStr;
					item.Click += new EventHandler( this.MenuClick_RestoreScale );
					if ( pane == null )
						item.Enabled = false;
					menuStrip.Items.Add( item );

					// if e.Cancel is set to false, the context menu does not display
					// it is initially set to false because the context menu has no items
					e.Cancel = false;

					// Provide Callback for User to edit the context menu
					//Revision: JCarpenter 10/06 - add ContextMenuObjectState objState
					if ( this.ContextMenuBuilder != null )
						this.ContextMenuBuilder( this, menuStrip, _menuClickPt, objState );
				}
			}
		}

		/// <summary>
		/// Handler for the "Copy" context menu item.  Copies the current image to a bitmap on the
		/// clipboard.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_Copy( System.Object sender, System.EventArgs e )
		{
			Copy( _isShowCopyMessage );
		}

		/// <summary>
		/// Handler for the "Copy" context menu item.  Copies the current image to a bitmap on the
		/// clipboard.
		/// </summary>
		/// <param name="isShowMessage">boolean value that determines whether or not a prompt will be
		/// displayed.  true to show a message of "Image Copied to ClipBoard".</param>
		public void Copy( bool isShowMessage )
		{
			if ( _masterPane != null )
			{
				//Clipboard.SetDataObject( _masterPane.GetImage(), true );

				// Threaded copy mode to avoid crash with MTA
				// Contributed by Dave Moor
				Thread ct = new Thread( new ThreadStart( this.ClipboardCopyThread ) );
				//ct.ApartmentState = ApartmentState.STA;
				ct.SetApartmentState( ApartmentState.STA );
				ct.Start();
				ct.Join();

				if ( isShowMessage )
				{
					string str = _resourceManager.GetString( "copied_to_clip" );
					//MessageBox.Show( "Image Copied to ClipBoard" );
					MessageBox.Show( str );
				}
			}
		}

		/// <summary>
		/// A threaded version of the copy method to avoid crash with MTA
		/// </summary>
		private void ClipboardCopyThread()
		{
			Clipboard.SetDataObject( _masterPane.GetImage(), true );
		}

		/// <summary>
		/// Handler for the "Save Image As" context menu item.  Copies the current image to the selected
		/// file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_SaveAs( System.Object sender, System.EventArgs e )
		{
			SaveAs();
		}

		/// <summary>
		/// Handler for the "Save Image As" context menu item.  Copies the current image to the selected
		/// file in either the Emf (vector), or a variety of Bitmap formats.
		/// </summary>
		/// <remarks>
		/// Note that <see cref="SaveAsBitmap" /> and <see cref="SaveAsEmf" /> methods are provided
		/// which allow for Bitmap-only or Emf-only handling of the "Save As" context menu item.
		/// </remarks>
		public void SaveAs()
		{
			if ( _masterPane != null )
			{
				_saveFileDialog.Filter =
					"Emf Format (*.emf)|*.emf|" +
					"PNG Format (*.png)|*.png|" +
					"Gif Format (*.gif)|*.gif|" +
					"Jpeg Format (*.jpg)|*.jpg|" +
					"Tiff Format (*.tif)|*.tif|" +
					"Bmp Format (*.bmp)|*.bmp";

				if ( _saveFileDialog.ShowDialog() == DialogResult.OK )
				{
					Stream myStream = _saveFileDialog.OpenFile();
					if ( myStream != null )
					{
						if ( _saveFileDialog.FilterIndex == 1 )
						{
							myStream.Close();
							_masterPane.GetMetafile().Save( _saveFileDialog.FileName );
						}
						else
						{
							ImageFormat format = ImageFormat.Png;
							if ( _saveFileDialog.FilterIndex == 3 )
								format = ImageFormat.Gif;
							else if ( _saveFileDialog.FilterIndex == 4 )
								format = ImageFormat.Jpeg;
							else if ( _saveFileDialog.FilterIndex == 5 )
								format = ImageFormat.Tiff;
							else if ( _saveFileDialog.FilterIndex == 6 )
								format = ImageFormat.Bmp;

							_masterPane.GetImage().Save( myStream, format );
							myStream.Close();
						}
					}
				}
			}
		}

		/// <summary>
		/// Handler for the "Save Image As" context menu item.  Copies the current image to the selected
		/// Bitmap file.
		/// </summary>
		/// <remarks>
		/// Note that this handler saves as a bitmap only.  The default handler is
		/// <see cref="SaveAs" />, which allows for Bitmap or EMF formats
		/// </remarks>
		public void SaveAsBitmap()
		{
			if ( _masterPane != null )
			{
				_saveFileDialog.Filter =
					"PNG Format (*.png)|*.png|" +
					"Gif Format (*.gif)|*.gif|" +
					"Jpeg Format (*.jpg)|*.jpg|" +
					"Tiff Format (*.tif)|*.tif|" +
					"Bmp Format (*.bmp)|*.bmp";

				if ( _saveFileDialog.ShowDialog() == DialogResult.OK )
				{
					ImageFormat format = ImageFormat.Png;
					if ( _saveFileDialog.FilterIndex == 2 )
						format = ImageFormat.Gif;
					else if ( _saveFileDialog.FilterIndex == 3 )
						format = ImageFormat.Jpeg;
					else if ( _saveFileDialog.FilterIndex == 4 )
						format = ImageFormat.Tiff;
					else if ( _saveFileDialog.FilterIndex == 5 )
						format = ImageFormat.Bmp;

					Stream myStream = _saveFileDialog.OpenFile();
					if ( myStream != null )
					{
						_masterPane.GetImage().Save( myStream, format );
						myStream.Close();
					}
				}
			}
		}

		/// <summary>
		/// Handler for the "Save Image As" context menu item.  Copies the current image to the selected
		/// Emf format file.
		/// </summary>
		/// <remarks>
		/// Note that this handler saves as an Emf format only.  The default handler is
		/// <see cref="SaveAs" />, which allows for Bitmap or EMF formats.
		/// </remarks>
		public void SaveAsEmf()
		{
			if ( _masterPane != null )
			{
				_saveFileDialog.Filter = "Emf Format (*.emf)|*.emf";

				if ( _saveFileDialog.ShowDialog() == DialogResult.OK )
				{
					Stream myStream = _saveFileDialog.OpenFile();
					if ( myStream != null )
					{
						myStream.Close();
						_masterPane.GetMetafile().Save( _saveFileDialog.FileName );
					}
				}
			}
		}

		/// <summary>
		/// Handler for the "Show Values" context menu item.  Toggles the <see cref="IsShowPointValues"/>
		/// property, which activates the point value tooltips.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_ShowValues( object sender, System.EventArgs e )
		{
			ToolStripMenuItem item = sender as ToolStripMenuItem;
			if ( item != null )
				this.IsShowPointValues = !item.Checked;
		}

		/// <summary>
		/// Handler for the "Set Scale to Default" context menu item.  Sets the scale ranging to
		/// full auto mode for all axes.
		/// </summary>
		/// <remarks>
		/// This method differs from the <see cref="ZoomOutAll" /> method in that it sets the scales
		/// to full auto mode.  The <see cref="ZoomOutAll" /> method sets the scales to their initial
		/// setting prior to any user actions (which may or may not be full auto mode).
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_RestoreScale( object sender, EventArgs e )
		{
			if ( _masterPane != null )
			{
				GraphPane pane = _masterPane.FindPane( _menuClickPt );
				RestoreScale( pane );
			}
		}

		/// <summary>
		/// Handler for the "Set Scale to Default" context menu item.  Sets the scale ranging to
		/// full auto mode for all axes.
		/// </summary>
		/// <remarks>
		/// This method differs from the <see cref="ZoomOutAll" /> method in that it sets the scales
		/// to full auto mode.  The <see cref="ZoomOutAll" /> method sets the scales to their initial
		/// setting prior to any user actions (which may or may not be full auto mode).
		/// </remarks>
		/// <param name="primaryPane">The <see cref="GraphPane" /> object which is to have the
		/// scale restored</param>
		public void RestoreScale( GraphPane primaryPane )
		{
			if ( primaryPane != null )
			{
				//Go ahead and save the old zoomstates, which provides an "undo"-like capability
				//ZoomState oldState = primaryPane.ZoomStack.Push( primaryPane, ZoomState.StateType.Zoom );
				ZoomState oldState = new ZoomState( primaryPane, ZoomState.StateType.Zoom );

				using ( Graphics g = this.CreateGraphics() )
				{
					if ( _isSynchronizeXAxes || _isSynchronizeYAxes )
					{
						foreach ( GraphPane pane in _masterPane._paneList )
						{
							pane.ZoomStack.Push( pane, ZoomState.StateType.Zoom );
							ResetAutoScale( pane, g );
						}
					}
					else
					{
						primaryPane.ZoomStack.Push( primaryPane, ZoomState.StateType.Zoom );
						ResetAutoScale( primaryPane, g );
					}

					// Provide Callback to notify the user of zoom events
					if ( this.ZoomEvent != null )
						this.ZoomEvent( this, oldState, new ZoomState( primaryPane, ZoomState.StateType.Zoom ) );

					//g.Dispose();
				}
				Refresh();
			}
		}

		private void ResetAutoScale( GraphPane pane, Graphics g )
		{
			pane.XAxis.ResetAutoScale( pane, g );
			pane.X2Axis.ResetAutoScale( pane, g );
			foreach ( YAxis axis in pane.YAxisList )
				axis.ResetAutoScale( pane, g );
			foreach ( Y2Axis axis in pane.Y2AxisList )
				axis.ResetAutoScale( pane, g );
		}

		/*
				public void RestoreScale( GraphPane primaryPane )
				{
					if ( primaryPane != null )
					{
						Graphics g = this.CreateGraphics();
						ZoomState oldState = new ZoomState( primaryPane, ZoomState.StateType.Zoom );
						//ZoomState newState = null;

						if ( _isSynchronizeXAxes || _isSynchronizeYAxes )
						{
							foreach ( GraphPane pane in _masterPane._paneList )
							{
								if ( pane == primaryPane )
								{
									pane.XAxis.ResetAutoScale( pane, g );
									foreach ( YAxis axis in pane.YAxisList )
										axis.ResetAutoScale( pane, g );
									foreach ( Y2Axis axis in pane.Y2AxisList )
										axis.ResetAutoScale( pane, g );
								}
							}
						}
						else
						{
							primaryPane.XAxis.ResetAutoScale( primaryPane, g );
							foreach ( YAxis axis in primaryPane.YAxisList )
								axis.ResetAutoScale( primaryPane, g );
							foreach ( Y2Axis axis in primaryPane.Y2AxisList )
								axis.ResetAutoScale( primaryPane, g );
						}

						// Provide Callback to notify the user of zoom events
						if ( this.ZoomEvent != null )
							this.ZoomEvent( this, oldState, new ZoomState( primaryPane, ZoomState.StateType.Zoom ) );

						g.Dispose();
						Refresh();
					}
				}
		*/
		/*
				public void ZoomOutAll( GraphPane primaryPane )
				{
					if ( primaryPane != null && !primaryPane.ZoomStack.IsEmpty )
					{
						ZoomState.StateType type = primaryPane.ZoomStack.Top.Type;

						ZoomState oldState = new ZoomState( primaryPane, type );
						//ZoomState newState = pane.ZoomStack.PopAll( pane );
						ZoomState newState = null;
						if ( _isSynchronizeXAxes || _isSynchronizeYAxes )
						{
							foreach ( GraphPane pane in _masterPane._paneList )
							{
								ZoomState state = pane.ZoomStack.PopAll( pane );
								if ( pane == primaryPane )
									newState = state;
							}
						}
						else
							newState = primaryPane.ZoomStack.PopAll( primaryPane );

						// Provide Callback to notify the user of zoom events
						if ( this.ZoomEvent != null )
							this.ZoomEvent( this, oldState, newState );

						Refresh();
					}
				}

		*/

		/// <summary>
		/// Handler for the "UnZoom/UnPan" context menu item.  Restores the scale ranges to the values
		/// before the last zoom or pan operation.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_ZoomOut( System.Object sender, System.EventArgs e )
		{
			if ( _masterPane != null )
			{
				GraphPane pane = _masterPane.FindPane( _menuClickPt );
				ZoomOut( pane );
			}
		}

		/// <summary>
		/// Handler for the "UnZoom/UnPan" context menu item.  Restores the scale ranges to the values
		/// before the last zoom, pan, or scroll operation.
		/// </summary>
		/// <remarks>
		/// Triggers a <see cref="ZoomEvent" /> for any type of undo (including pan, scroll, zoom, and
		/// wheelzoom).  This method will affect all the
		/// <see cref="GraphPane" /> objects in the <see cref="MasterPane" /> if
		/// <see cref="IsSynchronizeXAxes" /> or <see cref="IsSynchronizeYAxes" /> is true.
		/// </remarks>
		/// <param name="primaryPane">The primary <see cref="GraphPane" /> object which is to be
		/// zoomed out</param>
		public void ZoomOut( GraphPane primaryPane )
		{
			if ( primaryPane != null && !primaryPane.ZoomStack.IsEmpty )
			{
				ZoomState.StateType type = primaryPane.ZoomStack.Top.Type;

				ZoomState oldState = new ZoomState( primaryPane, type );
				ZoomState newState = null;
				if ( _isSynchronizeXAxes || _isSynchronizeYAxes )
				{
					foreach ( GraphPane pane in _masterPane._paneList )
					{
						ZoomState state = pane.ZoomStack.Pop( pane );
						if ( pane == primaryPane )
							newState = state;
					}
				}
				else
					newState = primaryPane.ZoomStack.Pop( primaryPane );

				// Provide Callback to notify the user of zoom events
				if ( this.ZoomEvent != null )
					this.ZoomEvent( this, oldState, newState );

				Refresh();
			}
		}

		/// <summary>
		/// Handler for the "Undo All Zoom/Pan" context menu item.  Restores the scale ranges to the values
		/// before all zoom and pan operations
		/// </summary>
		/// <remarks>
		/// This method differs from the <see cref="RestoreScale" /> method in that it sets the scales
		/// to their initial setting prior to any user actions.  The <see cref="RestoreScale" /> method
		/// sets the scales to full auto mode (regardless of what the initial setting may have been).
		/// </remarks>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void MenuClick_ZoomOutAll( System.Object sender, System.EventArgs e )
		{
			if ( _masterPane != null )
			{
				GraphPane pane = _masterPane.FindPane( _menuClickPt );
				ZoomOutAll( pane );
			}
		}

		/// <summary>
		/// Handler for the "Undo All Zoom/Pan" context menu item.  Restores the scale ranges to the values
		/// before all zoom and pan operations
		/// </summary>
		/// <remarks>
		/// This method differs from the <see cref="RestoreScale" /> method in that it sets the scales
		/// to their initial setting prior to any user actions.  The <see cref="RestoreScale" /> method
		/// sets the scales to full auto mode (regardless of what the initial setting may have been).
		/// </remarks>
		/// <param name="primaryPane">The <see cref="GraphPane" /> object which is to be zoomed out</param>
		public void ZoomOutAll( GraphPane primaryPane )
		{
			if ( primaryPane != null && !primaryPane.ZoomStack.IsEmpty )
			{
				ZoomState.StateType type = primaryPane.ZoomStack.Top.Type;

				ZoomState oldState = new ZoomState( primaryPane, type );
				//ZoomState newState = pane.ZoomStack.PopAll( pane );
				ZoomState newState = null;
				if ( _isSynchronizeXAxes || _isSynchronizeYAxes )
				{
					foreach ( GraphPane pane in _masterPane._paneList )
					{
						ZoomState state = pane.ZoomStack.PopAll( pane );
						if ( pane == primaryPane )
							newState = state;
					}
				}
				else
					newState = primaryPane.ZoomStack.PopAll( primaryPane );

				// Provide Callback to notify the user of zoom events
				if ( this.ZoomEvent != null )
					this.ZoomEvent( this, oldState, newState );

				Refresh();
			}
		}

	#endregion

	}
}