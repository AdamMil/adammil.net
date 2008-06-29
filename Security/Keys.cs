/*
GPG.net is a .NET interface to the GNU Privacy Guard (www.gnupg.org).
http://www.adammil.net/
Copyright (C) 2008 Adam Milazzo

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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AdamMil.Collections;

namespace AdamMil.Security.PGP
{

#region KeyCapability
/// <summary>Describes the capabilities of a key, but not necessarily the capabilities to which it can currently be put
/// to use by you. For instance, a key may be capable of encryption and signing, but if you don't have the private
/// portion, you cannot utilize that capability. Or, the key may have been disabled.
/// </summary>
[Flags]
public enum KeyCapability
{
  /// <summary>The key has no utility.</summary>
  None=0,
  /// <summary>The key can be used to encrypt data.</summary>
  Encrypt=1,
  /// <summary>The key can be used to sign data.</summary>
  Sign=2,
  /// <summary>The key can be used to certify other keys.</summary>
  Certify=4,
  /// <summary>The key can be used to authenticate its owners.</summary>
  Authenticate=8
}
#endregion

#region ReadOnlyClass
/// <summary>Represents a class that allows its properties to be set until <see cref="Finish"/> is called, at which
/// point the object becomes read-only.
/// </summary>
public abstract class ReadOnlyClass
{
  /// <include file="documentation.xml" path="/Security/ReadOnlyClass/Finish/*"/>
  public virtual void Finish()
  {
    finished = true;
  }

  /// <summary>Throws an exception if <see cref="Finish"/> has been called.</summary>
  protected void AssertNotFinished()
  {
    if(finished) throw new InvalidOperationException("This object has been finished, and the property is read only.");
  }

  bool finished;
}
#endregion

#region KeySignature
/// <summary>Represents a signature on a key.</summary>
public class KeySignature : ReadOnlyClass
{
  /// <summary>Gets or sets the time when the signature was created.</summary>
  public DateTime CreationTime
  {
    get { return creationTime; }
    set
    {
      AssertNotFinished();
      creationTime = value;
    }
  }

  /// <summary>Gets or sets whether this signature is exportable.</summary>
  public bool Exportable
  {
    get { return exportable; }
    set
    {
      AssertNotFinished();
      exportable = value;
    }
  }

  /// <summary>Gets or sets the fingerprint of the signing key.</summary>
  public string Fingerprint
  {
    get { return fingerprint; }
    set
    {
      AssertNotFinished();
      fingerprint = value;
    }
  }

  /// <summary>Gets or sets the ID of the signing key. The key ID is not guaranteed to be unique.</summary>
  public string KeyId
  {
    get { return keyId; }
    set
    {
      AssertNotFinished();
      keyId = value;
    }
  }

  /// <summary>Gets or sets the status of the signature. This is only guaranteed to be valid if
  /// <see cref="KeySignatures.Verify"/> was used during the retrieval of the key signatures.
  /// </summary>
  public SignatureStatus Status
  {
    get { return status; }
    set
    {
      AssertNotFinished();
      status = value;
    }
  }

  /// <summary>Gets or sets the user ID of the signer.</summary>
  public string SignerName
  {
    get { return signerName; }
    set
    {
      AssertNotFinished();
      signerName = value;
    }
  }

  /// <summary>Gets the trust level of the signature if the signature is a certification signature.</summary>
  public TrustLevel TrustLevel
  {
    get { return trustLevel; }
  }

  /// <summary>Gets or sets the signature type.</summary>
  public OpenPGPSignatureType Type
  {
    get { return type; }
    set
    {
      AssertNotFinished();
      type = value;
    }
  }

  /// <include file="documentation.xml" path="/Security/ReadOnlyClass/Finish/*"/>
  public override void Finish()
  {
    switch(type)
    {
      case OpenPGPSignatureType.PersonaCertification: trustLevel = TrustLevel.Never; break;
      case OpenPGPSignatureType.CasualCertification: trustLevel = TrustLevel.Marginal; break;
      case OpenPGPSignatureType.PositiveCertification: trustLevel = TrustLevel.Full; break;
      default: trustLevel = TrustLevel.Unknown; break;
    }

    base.Finish();
  }

  /// <include file="documentation.xml" path="/Security/Common/ToString/*"/>
  public override string ToString()
  {
    string str;
    if((status & SignatureStatus.SuccessMask) == SignatureStatus.Valid) str = "Valid ";
    else if((status & SignatureStatus.SuccessMask) == SignatureStatus.Error) str = "Error in ";
    else str = "Invalid ";

    str += type.ToString() + " signature";
    
    if(!string.IsNullOrEmpty(SignerName) || !string.IsNullOrEmpty(KeyId))
    {
      str += " by "+SignerName;
      if(!string.IsNullOrEmpty(KeyId)) str += string.IsNullOrEmpty(SignerName) ? "0x"+KeyId : " [0x"+KeyId+"]";
    }

    return str;
  }

  string fingerprint, keyId, signerName;
  DateTime creationTime;
  TrustLevel trustLevel;
  SignatureStatus status = SignatureStatus.Valid;
  OpenPGPSignatureType type = OpenPGPSignatureType.Unknown;
  bool exportable;
}
#endregion

#region UserId
/// <summary>Represents a user ID for a key. Some keys may be used by multiple people, or one person filling multiple
/// roles, or one person who changed his name or email address, and user IDs allow these multiple identity claims to be
/// associated with a key and individually trusted, revoked, etc. User IDs can be signed by people who testify to the
/// truthfulness and accuracy of the identity.
/// </summary>
/// <remarks>After the PGP system creates a <see cref="UserId"/> object and sets its properties, it should call
/// <see cref="Finish"/> to lock the property values, creating a read-only object.
/// </remarks>
public class UserId : ReadOnlyClass
{
  /// <summary>Gets or sets the calculated trust level of this user ID, which represents how much this user ID is
  /// trusted to be owned by the person named on it.
  /// </summary>
  public TrustLevel CalculatedTrust
  {
    get { return trustLevel; }
    set
    {
      AssertNotFinished();
      trustLevel = value;
    }
  }

  /// <summary>Gets or sets the date when this user ID was created.</summary>
  public DateTime CreationTime
  {
    get { return creationTime; }
    set
    {
      AssertNotFinished();
      creationTime = value; 
    }
  }

  /// <summary>Gets or sets the <see cref="PrimaryKey"/> to which this <see cref="UserId"/> belongs.</summary>
  public PrimaryKey Key
  {
    get { return key; }
    set
    {
      AssertNotFinished();
      key = value;
    }
  }

  /// <summary>Gets or sets the name of the user. The standard format is <c>NAME (COMMENT) &lt;EMAIL&gt;</c>, where
  /// the comment is optional. This format should be used with unless you have a compelling reason to do otherwise.
  /// </summary>
  public string Name
  {
    get { return name; }
    set 
    {
      AssertNotFinished();
      name = value; 
    }
  }

  /// <summary>Gets or sets whether this is the primary user ID of the key.</summary>
  public bool Primary
  {
    get { return primary; }
    set 
    {
      AssertNotFinished();
      primary = value; 
    }
  }

  /// <summary>Gets or sets whether this user ID has been revoked.</summary>
  public bool Revoked
  {
    get { return revoked; }
    set 
    {
      AssertNotFinished();
      revoked = value; 
    }
  }

  /// <summary>Gets or sets a read-only list of signatures on this user ID.</summary>
  public IReadOnlyList<KeySignature> Signatures
  {
    get { return sigs; }
    set
    {
      AssertNotFinished();
      sigs = value;
    }
  }

  /// <include file="documentation.xml" path="/Security/ReadOnlyClass/Finish/*"/>
  public override void Finish()
  {
    if(key == null) throw new InvalidOperationException("The Key property is not set.");
    if(sigs == null) throw new InvalidOperationException("The Signatures property is not set.");
    base.Finish();
  }

  /// <include file="documentation.xml" path="/Security/Common/ToString/*"/>
  public override string ToString()
  {
    return string.IsNullOrEmpty(Name) ? "Unknown user" : Name;
  }

  PrimaryKey key;
  string name;
  IReadOnlyList<KeySignature> sigs;
  DateTime creationTime;
  TrustLevel trustLevel;
  bool primary, revoked;
}
#endregion

#region Key
/// <summary>A base class for OpenPGP keys.</summary>
public abstract class Key : ReadOnlyClass
{
  /// <summary>Gets or sets the calculated trust level of this key, which represents how strongly this key is believed
  /// to be owned by at least one of its user IDs.
  /// </summary>
  public TrustLevel CalculatedTrust
  {
    get { return calculatedTrust; }
    set 
    {
      AssertNotFinished();
      calculatedTrust = value; 
    }
  }

  /// <summary>Gets or sets the capabilities of this key. This value represents the original capabilities of the key,
  /// not necessarily what a particular person will be able to do with it.
  /// </summary>
  public KeyCapability Capabilities
  {
    get { return capabilities; }
    set 
    {
      AssertNotFinished();
      capabilities = value; 
    }
  }

  /// <summary>Gets or sets the time when the key was created.</summary>
  public DateTime CreationTime
  {
    get { return creationTime; }
    set 
    {
      AssertNotFinished();
      creationTime = value; 
    }
  }

  /// <summary>Gets or sets the time when the key will expire, or null if it has no expiration.</summary>
  public DateTime? ExpirationTime
  {
    get { return expirationTime; }
    set 
    {
      AssertNotFinished();
      expirationTime = value; 
    }
  }

  /// <summary>Gets or sets whether the key has expired.</summary>
  public bool Expired
  {
    get { return expired; }
    set 
    {
      AssertNotFinished();
      expired = value; 
    }
  }

  /// <summary>Gets or sets the fingerprint of the key. The fingerprint can be used as a unique key ID, but there is a
  /// miniscule chance that two different keys will have the same fingerprint.
  /// </summary>
  public string Fingerprint
  {
    get { return fingerprint; }
    set 
    {
      AssertNotFinished();
      fingerprint = value; 
    }
  }

  /// <summary>Gets or sets whether the key is invalid (for instance, due to a missing self-signature).</summary>
  public bool Invalid
  {
    get { return invalid; }
    set 
    {
      AssertNotFinished();
      invalid = value; 
    }
  }

  /// <summary>Gets or sets the ID of the key. Note that the key ID is not guaranteed to be unique. For a more unique
  /// ID, use the <see cref="Fingerprint"/>.
  /// </summary>
  public string KeyId
  {
    get { return keyId; }
    set 
    {
      AssertNotFinished();
      keyId = value; 
    }
  }

  /// <summary>Gets or sets the name of the key type, or null if the key type could not be determined.</summary>
  public string KeyType
  {
    get { return keyType; }
    set 
    {
      AssertNotFinished();
      keyType = value; 
    }
  }

  /// <include file="documentation.xml" path="/Security/Key/Keyring/*"/>
  public abstract Keyring Keyring
  {
    get; set;
  }

  /// <summary>Gets or sets the length of the key, in bits.</summary>
  public int Length
  {
    get { return length; }
    set 
    {
      AssertNotFinished();
      length = value; 
    }
  }

  /// <summary>Gets or sets whether the key has been revoked.</summary>
  public bool Revoked
  {
    get { return revoked; }
    set 
    {
      AssertNotFinished();
      revoked = value; 
    }
  }

  /// <summary>Gets or sets whether this is a secret key.</summary>
  public bool Secret
  {
    get { return secret; }
    set
    {
      AssertNotFinished();
      secret = value;
    }
  }

  /// <summary>Gets or sets a read-only list of signatures on this user ID.</summary>
  public IReadOnlyList<KeySignature> Signatures
  {
    get { return sigs; }
    set
    {
      AssertNotFinished();
      sigs = value;
    }
  }

  /// <include file="documentation.xml" path="/Security/ReadOnlyClass/Finish/*"/>
  public override void Finish()
  {
    if(sigs == null) throw new InvalidOperationException("The Signatures property has not been set.");
    base.Finish();
  }

  /// <summary>Gets or sets the primary key associated with this key, or the current key if it is a primary key.</summary>
  public abstract PrimaryKey GetPrimaryKey();

  /// <include file="documentation.xml" path="/Security/Common/ToString/*"/>
  public override string ToString()
  {
    return string.IsNullOrEmpty(KeyId) ? Fingerprint : KeyId;
  }

  string keyId, keyType, fingerprint;
  IReadOnlyList<KeySignature> sigs;
  DateTime? expirationTime;
  DateTime creationTime;
  int length;
  KeyCapability capabilities;
  TrustLevel calculatedTrust;
  bool invalid, revoked, expired, secret;
}
#endregion

#region PrimaryKey
/// <summary>An OpenPGP primary key. In OpenPGP, the keys on a keyring are primary keys. Each primary key can have an
/// arbitrary number of <see cref="UserId">user IDs</see> and <see cref="Subkey">subkeys</see> associated with it. Both
/// primary keys and subkeys can be used to sign and encrypt, depending on their individual capabilities, but typically
/// the roles of the keys are divided so that the primary key is only used for signing while the subkeys are only used
/// for encryption.
/// </summary>
public class PrimaryKey : Key
{
  /// <summary>Gets or sets whether the key has been disabled, indicating that it should not be used. Because the
  /// enabled status is not stored within the key, a key can be disabled by anyone, but it will only be disabled for
  /// that person. It can be reenabled at any time.
  /// </summary>
  public bool Disabled
  {
    get { return disabled; }
    set
    {
      AssertNotFinished();
      disabled = value;
    }
  }

  /// <include file="documentation.xml" path="/Security/Key/Keyring/*"/>
  public override Keyring Keyring
  {
    get { return keyring; }
    set
    {
      AssertNotFinished();
      keyring = value;
    }
  }

  /// <summary>Gets or sets the extent to which the owner(s) of a key are trusted to validate the ownership
  /// of other people's keys.
  /// </summary>
  public TrustLevel OwnerTrust
  {
    get { return ownerTrust; }
    set
    {
      AssertNotFinished();
      ownerTrust = value;
    }
  }

  /// <summary>Gets the primary user ID for this key, or null if no user ID has been marked as primary.</summary>
  public UserId PrimaryUserId
  {
    get { return primaryUserId; }
  }

  /// <summary>Gets or sets a read-only list of subkeys of this primary key.</summary>
  public IReadOnlyList<Subkey> Subkeys
  {
    get { return subkeys; }
    set
    {
      AssertNotFinished();
      subkeys = value;
    }
  }

  /// <summary>Gets or sets the combined capabilities of this primary key and its subkeys.</summary>
  public KeyCapability TotalCapabilities
  {
    get { return totalCapabilities; }
    set
    {
      AssertNotFinished();
      totalCapabilities = value;
    }
  }

  /// <summary>Gets or sets a read-only list of user IDs associated with this primary key.</summary>
  public IReadOnlyList<UserId> UserIds
  {
    get { return userIds; }
    set
    {
      AssertNotFinished();
      userIds = value;
    }
  }

  /// <include file="documentation.xml" path="/Security/Key/Finish/*"/>
  public override void Finish()
  {
    if(subkeys == null) throw new InvalidOperationException("The Subkeys property is not set.");
    if(userIds == null || userIds.Count == 0)
    {
      throw new InvalidOperationException("The UserIds property is not set, or is empty.");
    }

    primaryUserId = null;
    foreach(UserId user in UserIds)
    {
      if(user.Primary)
      {
        if(primaryUserId != null) throw new InvalidOperationException("There are multiple primary user ids.");
        primaryUserId = user;
      }
    }

    base.Finish();
  }

  /// <summary>Returns this key.</summary>
  public override PrimaryKey GetPrimaryKey()
  {
    return this;
  }

  /// <include file="documentation.xml" path="/Security/Common/ToString/*"/>
  public override string ToString()
  {
    string str = base.ToString();
    if(PrimaryUserId != null) str += " " + PrimaryUserId.ToString();
    return str;
  }

  IReadOnlyList<Subkey> subkeys;
  IReadOnlyList<UserId> userIds;
  UserId primaryUserId;
  Keyring keyring;
  KeyCapability totalCapabilities;
  TrustLevel ownerTrust;
  bool disabled;
}
#endregion

#region Subkey
/// <summary>Represents a subkey of a primary key. See <see cref="PrimaryKey"/> for a more thorough description.</summary>
public class Subkey : Key
{
  /// <include file="documentation.xml" path="/Security/Key/Keyring/*"/>
  public override Keyring Keyring
  {
    get { return PrimaryKey != null ? PrimaryKey.Keyring : null; }
    set { throw new NotSupportedException("To change a subkey's keyring, set the keyring of its primary key."); }
  }

  /// <summary>Gets or sets the primary key that owns this subkey.</summary>
  public PrimaryKey PrimaryKey
  {
    get { return primaryKey; }
    set 
    {
      AssertNotFinished();
      primaryKey = value; 
    }
  }

  /// <include file="documentation.xml" path="/Security/Key/Finish/*"/>
  public override void Finish()
  {
    if(primaryKey == null) throw new InvalidOperationException("The PrimaryKey property has not been set.");
    base.Finish();
  }

  /// <summary>Gets the primary key that owns this subkey.</summary>
  public override PrimaryKey GetPrimaryKey()
  {
    return primaryKey;
  }

  /// <include file="documentation.xml" path="/Security/Common/ToString/*"/>
  public override string ToString()
  {
    string str = base.ToString();
    if(PrimaryKey != null && PrimaryKey.PrimaryUserId != null) str += " " + PrimaryKey.PrimaryUserId.ToString();
    return str;
  }

  PrimaryKey primaryKey;
}
#endregion

#region KeyCollection
/// <summary>A collection of <see cref="Key"/> objects.</summary>
public class KeyCollection : Collection<Key>
{
  /// <summary>Initializes a new <see cref="KeyCollection"/> with no required key capabilities.</summary>
  public KeyCollection() { }
  
  /// <summary>Initializes a new <see cref="KeyCollection"/> with the given set of required key capabilities.</summary>
  public KeyCollection(KeyCapability requiredCapabilities)
  {
    this.requiredCapabilities = requiredCapabilities;
  }

  /// <summary>Called when a new item is about to be inserted.</summary>
  protected override void InsertItem(int index, Key item)
  {
    ValidateKey(item);
    base.InsertItem(index, item);
  }

  /// <summary>Called when an item is about to be changed.</summary>
  protected override void SetItem(int index, Key item)
  {
    ValidateKey(item);
    base.SetItem(index, item);
  }

  /// <summary>Called to verify that the key matches is allowed in the collection.</summary>
  protected void ValidateKey(Key key)
  {
    if(key == null) throw new ArgumentNullException();

    PrimaryKey primaryKey = key as PrimaryKey;
    KeyCapability capabilities = primaryKey != null ? primaryKey.TotalCapabilities : key.Capabilities;

    if((capabilities & requiredCapabilities) != requiredCapabilities)
    {
      throw new ArgumentException("The key does not have all of the required capabilities: " +
                                  requiredCapabilities.ToString());
    }
  }

  KeyCapability requiredCapabilities;
}
#endregion

#region Keyring
/// <summary>Represents a keyring, which is composed of a public keyring file and a secret keyring file. The public
/// file stores public keys and their signatures and attributes, while the secret keyring file stores secret keys.
/// It is possible to have a keyring with only a public keyring file (indicating that the secret keys are missing), but
/// not to have a secret file without a public file.
/// </summary>
public class Keyring
{
  /// <summary>Initializes a new <see cref="Keyring"/> with the given public and secret filenames.</summary>
  /// <param name="publicFile">The name of the public file, which is required.</param>
  /// <param name="secretFile">The name of the secret file, which is optional.</param>
  public Keyring(string publicFile, string secretFile)
  {
    if(string.IsNullOrEmpty(publicFile)) throw new ArgumentException("The public portion of a keyring is required.");
    this.publicFile = publicFile;
    this.secretFile = string.IsNullOrEmpty(secretFile) ? null : secretFile;
  }

  /// <summary>Gets the name of the public keyring file.</summary>
  public string PublicFile
  {
    get { return publicFile; }
  }

  /// <summary>Gets the name of the secret keyring file.</summary>
  public string SecretFile
  {
    get { return secretFile; }
  }

  string publicFile, secretFile;
}
#endregion

} // namespace AdamMil.Security.PGP