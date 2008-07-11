/*
AdamMil.Security.UI is a .NET library providing common user interface widgets
for OpenPGP-based software.
Copyright (C) 2008 Adam Milazzo (http://www.adammil.net/)

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.ComponentModel;
using System.Windows.Forms;
using AdamMil.Security.PGP;

namespace AdamMil.Security.UI
{

public partial class KeySigningForm : Form
{
  public KeySigningForm()
  {
    InitializeComponent();
  }

  public KeySigningForm(PrimaryKey[] keysToSign, PrimaryKey[] signingKeys) : this()
  {
    Initialize(keysToSign, signingKeys);
  }

  [Browsable(false)]
  public KeySigningOptions KeySigningOptions
  {
    get 
    {
      CertificationLevel certLevel;
      if(rbNone.Checked) certLevel = CertificationLevel.None;
      else if(rbCasual.Checked) certLevel = CertificationLevel.Casual;
      else if(rbRigorous.Checked) certLevel = CertificationLevel.Rigorous;
      else certLevel = CertificationLevel.Undisclosed;

      return new KeySigningOptions(certLevel, !chkLocal.Checked); 
    }
  }

  [Browsable(false)]
  public PrimaryKey SelectedSigningKey
  {
    get { return ((KeyItem)signingKeys.SelectedItem).Value; }
  }

  public void Initialize(PrimaryKey[] keysToSign, PrimaryKey[] signingKeys)
  {
    if(keysToSign == null || signingKeys == null) throw new ArgumentNullException();
    if(keysToSign.Length == 0) throw new ArgumentException("No keys to sign were given.");
    if(signingKeys.Length == 0) throw new ArgumentException("No signing keys were given.");

    this.signedKeys.Items.Clear();
    this.signingKeys.Items.Clear();
    foreach(PrimaryKey key in keysToSign) this.signedKeys.Items.Add(new KeyItem(key));
    foreach(PrimaryKey key in signingKeys) this.signingKeys.Items.Add(new KeyItem(key));

    this.signingKeys.SelectedIndex = 0;
  }

  void rbPoor_CheckedChanged(object sender, EventArgs e)
  {
    if(((RadioButton)sender).Checked) chkLocal.Checked = true;
  }
}

} // namespace AdamMil.Security.UI