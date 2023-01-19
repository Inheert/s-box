#region assembly Sandbox.Game, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// emplacement inconnu
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using NativeEngine;
using Sandbox.Internal;

namespace Sandbox
{
    //
    // Résumé :
    //     A model entity that can also play animations and use animation graphs.
    [Library("animated_entity")]
    [Title("Animated Entity")]
    [Icon("directions_run")]
    [Editor("animated_entity")]
    public class AnimatedEntity : ModelEntity
    {
        //
        // Résumé :
        //     Enumeration that describes how the AnimGraph tag state changed. Used in Sandbox.AnimatedEntity.OnAnimGraphTag(System.String,Sandbox.AnimatedEntity.AnimGraphTagEvent).
        public enum AnimGraphTagEvent
        {
            //
            // Résumé :
            //     Tag was activated and deactivated on the same frame
            Fired,
            //
            // Résumé :
            //     The tag has become active
            Start,
            //
            // Résumé :
            //     The tag has become inactive
            End
        }

        internal CBaseAnimating serverAnimating;

        internal C_BaseAnimating clientAnimating;

        private AnimGraphDirectPlayback _directPlayback;

        internal override string NativeEntityClass => "baseanimating";

        //
        // Résumé :
        //     Allows playback of sequences directly, rather than using an animation graph.
        //     Requires Sandbox.AnimatedEntity.UseAnimGraph disabled if the entity has one.
        //     Also see Sandbox.AnimatedEntity.AnimateOnServer.
        public AnimationSequence CurrentSequence { get; private set; }

        //
        // Résumé :
        //     Allows the entity to not use the anim graph so it can play sequences directly
        public bool UseAnimGraph
        {
            get
            {
                if (!clientAnimating.IsNull)
                {
                    return clientAnimating.GetShouldUseAnimGraph();
                }

                if (!serverAnimating.IsNull)
                {
                    return serverAnimating.GetShouldUseAnimGraph();
                }

                return false;
            }
            set
            {
                if (!clientAnimating.IsNull)
                {
                    clientAnimating.SetShouldUseAnimGraph(value);
                }

                if (!serverAnimating.IsNull)
                {
                    serverAnimating.SetShouldUseAnimGraph(value);
                }
            }
        }

        //
        // Résumé :
        //     Override the anim graph this entity uses
        public AnimationGraph AnimGraph
        {
            get
            {
                if (clientModel.IsValid)
                {
                    return AnimationGraph.FromNative(clientAnimating.GetOverrideGraph());
                }

                if (serverModel.IsValid)
                {
                    return AnimationGraph.FromNative(serverAnimating.GetOverrideGraph());
                }

                return null;
            }
            set
            {
                AssertNotPreSpawn("AnimGraph");
                if (Game.IsClient && !base.IsClientOnly)
                {
                    throw new Exception("Trying to SetAnimGraph client side on networked entity");
                }

                if (value != null && !value.IsError)
                {
                    Precache.Add(value.Name);
                }

                if (clientModel.IsValid)
                {
                    clientAnimating.SetOverrideGraph(value?.native ?? default(HAnimationGraph));
                }

                if (serverModel.IsValid)
                {
                    serverAnimating.SetOverrideGraph(value?.native ?? default(HAnimationGraph));
                }
            }
        }

        //
        // Résumé :
        //     Whether this entity should animate on the server via Sandbox.AnimatedEntity.CurrentSequence.
        public bool AnimateOnServer
        {
            get
            {
                return HasEntityEffect(EntityEffects.AnimatedOnServer);
            }
            set
            {
                SetEntityEffects(EntityEffects.AnimatedOnServer, value);
            }
        }

        //
        // Résumé :
        //     Playback rate of the animations on this entity
        public float PlaybackRate
        {
            get
            {
                if (!clientAnimating.IsNull)
                {
                    return clientAnimating.GetPlaybackRate();
                }

                if (!serverAnimating.IsNull)
                {
                    return serverAnimating.GetPlaybackRate();
                }

                return 0f;
            }
            set
            {
                if (!clientAnimating.IsNull)
                {
                    clientAnimating.SetPlaybackRate(value);
                }

                if (!serverAnimating.IsNull)
                {
                    serverAnimating.SetPlaybackRate(value);
                }
            }
        }

        //
        // Résumé :
        //     Experimental root motion velocity for anim graphs that use root motion
        public Vector3 RootMotion
        {
            get
            {
                if (!clientAnimating.IsNull)
                {
                    return clientAnimating.GetRootMotion();
                }

                if (!serverAnimating.IsNull)
                {
                    return serverAnimating.GetRootMotion();
                }

                return default(Vector3);
            }
        }

        //
        // Résumé :
        //     Experimental root motion angle velocity for anim graphs that use root motion
        public float RootMotionAngle
        {
            get
            {
                if (!clientAnimating.IsNull)
                {
                    return clientAnimating.GetRootMotionAngle();
                }

                if (!serverAnimating.IsNull)
                {
                    return serverAnimating.GetRootMotionAngle();
                }

                return 0f;
            }
        }

        //
        // Résumé :
        //     Access this entity's direct playback. Direct playback is used to control the
        //     direct playback node in an animgraph to play sequences directly in code
        public AnimGraphDirectPlayback DirectPlayback
        {
            get
            {
                if (_directPlayback == null)
                {
                    _directPlayback = new EntityDirectPlayback(this);
                }

                return _directPlayback;
            }
        }

        internal override void OnNativeEntity(CEntityInstance ent)
        {
            base.OnNativeEntity(ent);
            if (Game.IsClient)
            {
                clientAnimating = (C_BaseAnimating)clientEnt;
                if (clientAnimating.IsNull)
                {
                    throw new Exception("clientAnimating is null");
                }
            }

            if (Game.IsServer)
            {
                serverAnimating = (CBaseAnimating)serverEnt;
                if (serverAnimating.IsNull)
                {
                    throw new Exception("serverAnimating is null");
                }
            }

            CurrentSequence = new AnimationSequence(this);
        }

        public AnimatedEntity()
        {
        }

        public AnimatedEntity(string modelName)
        {
            SetModel(modelName);
        }

        public AnimatedEntity(string modelName, IEntity parent)
        {
            SetModel(modelName);
            SetParent(parent as Entity, boneMerge: true);
        }

        //
        // Résumé :
        //     Override the anim graph this entity uses
        public void SetAnimGraph(string name)
        {
            AnimGraph = AnimationGraph.Load(name);
        }

        //
        // Résumé :
        //     Whether this entity's model has an anim graph or not
        public bool HasAnimGraph()
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.HasAnimGraph();
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.HasAnimGraph();
            }

            return false;
        }

        //
        // Résumé :
        //     Get the duration of a sequence by name
        public float GetSequenceDuration(string sequenceName)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.Script_SequenceDuration(sequenceName);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.Script_SequenceDuration(sequenceName);
            }

            return 0f;
        }

        //
        // Résumé :
        //     Check whether a sequence is valid by name
        public bool IsValidSequence(string sequenceName)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.IsValidSequence(sequenceName);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.IsValidSequence(sequenceName);
            }

            return false;
        }

        //
        // Résumé :
        //     Retrieve parameter value of currently active Animation Graph.
        //
        // Paramètres :
        //   name:
        //     Name of the parameter to look up value of.
        //
        // Retourne :
        //     The value of given parameter.
        public bool GetAnimParameterBool(string name)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.GetBoolGraphParameter(name);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.GetBoolGraphParameter(name);
            }

            return false;
        }

        public float GetAnimParameterFloat(string name)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.GetFloatGraphParameter(name);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.GetFloatGraphParameter(name);
            }

            return 0f;
        }

        public Vector3 GetAnimParameterVector(string name)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.GetVectorGraphParameter(name);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.GetVectorGraphParameter(name);
            }

            return default(Vector3);
        }

        public int GetAnimParameterInt(string name)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.GetIntGraphParameter(name);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.GetIntGraphParameter(name);
            }

            return 0;
        }

        public Rotation GetAnimParameterRotation(string name)
        {
            if (!clientAnimating.IsNull)
            {
                return clientAnimating.GetRotationGraphParameter(name);
            }

            if (!serverAnimating.IsNull)
            {
                return serverAnimating.GetRotationGraphParameter(name);
            }

            return default(Rotation);
        }

        //
        // Résumé :
        //     Sets the animation graph parameter.
        //
        // Paramètres :
        //   name:
        //     Name of the parameter to set.
        //
        //   value:
        //     Value to set.
        public void SetAnimParameter(string name, bool value)
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.SetGraphParameter(name, value);
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.SetGraphParameter(name, value);
            }
        }

        public void SetAnimParameter(string name, float value)
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.SetGraphParameter(name, value);
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.SetGraphParameter(name, value);
            }
        }

        public void SetAnimParameter(string name, Vector3 value)
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.SetGraphParameter(name, value);
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.SetGraphParameter(name, value);
            }
        }

        public void SetAnimParameter(string name, Rotation value)
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.SetGraphParameter(name, value);
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.SetGraphParameter(name, value);
            }
        }

        public void SetAnimParameter(string name, int value)
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.SetGraphParameter(name, value);
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.SetGraphParameter(name, value);
            }
        }

        public void SetAnimParameter(string name, Transform value)
        {
            SetAnimParameter(name + ".position", value.Position);
            SetAnimParameter(name + ".rotation", value.Rotation);
        }

        //
        // Résumé :
        //     Converts value to vector local to this entity's eyepos and passes it to SetAnimVector
        public void SetAnimLookAt(string name, Vector3 eyePositionInWorld, Vector3 lookatPositionInWorld)
        {
            Vector3 value = (lookatPositionInWorld - eyePositionInWorld) * Rotation.Inverse;
            SetAnimParameter(name, value);
        }

        //
        // Résumé :
        //     Reset all animgraph parameters to their default values
        public void ResetAnimParameters()
        {
            if (!clientAnimating.IsNull)
            {
                clientAnimating.ResetGraphParameters();
            }

            if (!serverAnimating.IsNull)
            {
                serverAnimating.ResetGraphParameters();
            }
        }

        internal override void InternalDestruct()
        {
            base.InternalDestruct();
            serverAnimating = IntPtr.Zero;
            clientAnimating = IntPtr.Zero;
        }

        //
        // Résumé :
        //     Called when a new animation sequence is set
        protected virtual void OnNewSequence()
        {
        }

        internal override void InternalOnNewSequence()
        {
            OnNewSequence();
        }

        //
        // Résumé :
        //     Called when an animation sequence has finished or looped
        //
        // Paramètres :
        //   looped:
        //     If the animation was restarted rather than stopped.
        protected virtual void OnSequenceFinished(bool looped)
        {
        }

        internal override void InternalOnSequenceFinished(bool looped)
        {
            OnSequenceFinished(looped);
        }

        //
        // Résumé :
        //     Called when the anim graph of this entity has a tag change. This will be called
        //     only for "Status" type tags.
        //
        // Paramètres :
        //   tag:
        //     The name of the tag that has changed its state, as it is defined in the AnimGraph.
        //
        //   fireMode:
        //     Describes how the state of the tag has changed.
        protected virtual void OnAnimGraphTag(string tag, AnimGraphTagEvent fireMode)
        {
        }

        internal override void InternalOnAnimGraphTag(string tag, int fireMode)
        {
            OnAnimGraphTag(tag, (AnimGraphTagEvent)fireMode);
        }

        //
        // Résumé :
        //     An anim graph has been created for this entity. You will want to set up initial
        //     AnimGraph parameters here.
        protected virtual void OnAnimGraphCreated()
        {
        }

        internal override void InternalOnAnimGraphCreated()
        {
            OnAnimGraphCreated();
        }

        //
        // Résumé :
        //     Called when an animation-driven footstep sound needs to be played.
        //
        // Paramètres :
        //   leftFoot:
        //     true = left foot, false = right foot
        //
        //   position:
        //     Position of the footstep in the world.
        protected virtual void OnAnimFootstep(bool leftFoot, Vector3 position)
        {
            GlobalGameNamespace.DebugOverlay.Sphere(position, 2f, leftFoot ? Color.Red : Color.Yellow, 10f, depthTest: false);
        }

        internal override void InternalOnAnimFootstepTag(bool left, Vector3 position)
        {
            OnAnimFootstep(left, position);
        }

        //
        // Résumé :
        //     Called when an animation-driven sound needs to be played.
        //
        // Paramètres :
        //   sound:
        //     Sound that needs to be played
        //
        //   attachment:
        //     Preferred attachment point of the sound.
        protected virtual void OnAnimSound(string sound, string attachment)
        {
            PlaySound(sound, attachment);
        }

        internal override void InternalOnAnimSound(string sound, string attachment)
        {
            OnAnimSound(sound, attachment);
        }
    }
}
#if false // Journal de décompilation
'170' éléments dans le cache
------------------
Résoudre : 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Runtime.dll'
------------------
Résoudre : 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Sandbox.Engine, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'd:\steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.dll'
------------------
Résoudre : 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Sandbox.System, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'd:\steam\steamapps\common\sbox\bin\managed\Sandbox.System.dll'
------------------
Résoudre : 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Runtime.InteropServices.dll'
------------------
Résoudre : 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Introuvable par le nom : 'Sandbox.Access, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
------------------
Résoudre : 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Collections.dll'
------------------
Résoudre : 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Sandbox.Reflection, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'd:\steam\steamapps\common\sbox\bin\managed\Sandbox.Reflection.dll'
------------------
Résoudre : 'System.ComponentModel.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.ComponentModel.Primitives, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.ComponentModel.Primitives.dll'
------------------
Résoudre : 'System.Text.Json, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Un seul assembly trouvé : 'System.Text.Json, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Text.Json.dll'
------------------
Résoudre : 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Sandbox.Event, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'd:\steam\steamapps\common\sbox\bin\managed\Sandbox.Event.dll'
------------------
Résoudre : 'System.ComponentModel.Annotations, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.ComponentModel.Annotations, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.ComponentModel.Annotations.dll'
------------------
Résoudre : 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Introuvable par le nom : 'System.Speech, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Résoudre : 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Threading, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Threading.dll'
------------------
Résoudre : 'System.Threading.Channels, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Un seul assembly trouvé : 'System.Threading.Channels, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Threading.Channels.dll'
------------------
Résoudre : 'System.Net.WebSockets.Client, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Net.WebSockets.Client, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Net.WebSockets.Client.dll'
------------------
Résoudre : 'System.Net.WebSockets, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Net.WebSockets, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Net.WebSockets.dll'
------------------
Résoudre : 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Introuvable par le nom : 'Facebook.Yoga, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Résoudre : 'System.Text.RegularExpressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Text.RegularExpressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Text.RegularExpressions.dll'
------------------
Résoudre : 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
Introuvable par le nom : 'SkiaSharp, Version=2.80.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756'
------------------
Résoudre : 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
Introuvable par le nom : 'Topten.RichTextKit, Version=0.4.148.0, Culture=neutral, PublicKeyToken=null'
------------------
Résoudre : 'Sandbox.Bind, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Sandbox.Bind, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'd:\steam\steamapps\common\sbox\bin\managed\Sandbox.Bind.dll'
------------------
Résoudre : 'System.Collections.Immutable, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Collections.Immutable, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Collections.Immutable.dll'
------------------
Résoudre : 'Microsoft.CodeAnalysis, Version=4.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Introuvable par le nom : 'Microsoft.CodeAnalysis, Version=4.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Résoudre : 'Microsoft.CodeAnalysis.CSharp, Version=4.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Introuvable par le nom : 'Microsoft.CodeAnalysis.CSharp, Version=4.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
------------------
Résoudre : 'System.ObjectModel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.ObjectModel, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.ObjectModel.dll'
------------------
Résoudre : 'System.Net.Http, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Net.Http, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Net.Http.dll'
------------------
Résoudre : 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Linq.dll'
------------------
Résoudre : 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Collections.Concurrent, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Collections.Concurrent.dll'
------------------
Résoudre : 'System.Numerics.Vectors, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Numerics.Vectors, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Numerics.Vectors.dll'
------------------
Résoudre : 'System.Memory, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Un seul assembly trouvé : 'System.Memory, Version=7.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Memory.dll'
------------------
Résoudre : 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Threading.Thread, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Threading.Thread.dll'
------------------
Résoudre : 'System.Console, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Console, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.Console.dll'
------------------
Résoudre : 'System.ComponentModel.EventBasedAsync, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.ComponentModel.EventBasedAsync, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\7.0.2\ref\net7.0\System.ComponentModel.EventBasedAsync.dll'
#endif
