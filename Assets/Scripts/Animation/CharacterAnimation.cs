using UnityEngine;
using System.Collections;

[AddComponentMenu( "Animation/Character Animation" )]
public class CharacterAnimation : MonoBehaviour
{
    // This defines the base length of time between each frame for looping animations.
    // Because this can be modified for attack animations, etc., we will keep a private
    // copy as well for all our actual calculations.
    public float FrameTime;

    // Animation maintenance fields.
    private bool paused;
    private float frameTime;
    private float frameTimer;
    private int currentFrame;
    private bool looping;
    private bool loopingForward;
    private bool flashing;
    public float DimAmount;
    private bool dim;
    private bool fadingOut;
    private float fadeOutTime;
    private float fadeOutTimer;

    // Texture loops.
    public Texture[] Idle;
    public Texture[] WalkCycle;
    public Texture[] Punch;
    public Texture[] Kick;
    // public Texture[] Dying;
    private Texture[] texLoop;


    // Initialization.
    void Start()
    {
        paused = false;
        fadingOut = false;
        fadeOutTime = GetComponentInParent<CharacterStatus>().DeathTime;
        fadeOutTimer = 0f;
        SetAnimation( "Idle" );
    }


    // Pause messages.
    public void OnPauseGame()
    {
        paused = true;
    }

    public void OnUnpauseGame()
    {
        paused = false;
    }


    // Animation setup.
    public void SetAnimation( string animationName )
    {
        frameTime = FrameTime;
        frameTimer = 0f;
        currentFrame = 0;
        looping = true;
        loopingForward = true;

        if( animationName == "Idle" )
            texLoop = Idle;
        else if( animationName == "Walk" )
            texLoop = WalkCycle;
        else if( animationName == "Dying" )
        {
            texLoop = Idle;
            DimAmount = 0f;
            dim = true;
            fadingOut = true;
        }
        else if( animationName == "Flashing" )
        {
            flashing = true;
            dim = true;
        }
        else if( animationName == "StopFlashing" )
        {
            flashing = false;
            dim = false;
        }
        else
        {
            Debug.LogError( "Animation \"" + animationName + "\" not found!" );
            texLoop = Idle;
        }

        UpdateMaterial();
    }

    public void SetAttackAnimation( GameObject attackObject )
    {
        var attack = attackObject.GetComponent<AttackDetail>();
        frameTime = attack.AttackDelay;
        frameTimer = 0f;
        currentFrame = 0;
        looping = false;

        if( attack.AnimationName == "Punch" )
            texLoop = Punch;
        else if( attack.AnimationName == "Kick" )
            texLoop = Kick;
        else
        {
            Debug.LogError( "Animation \"" + attack.AnimationName + "\" not found!" );
            SetAnimation( "Idle" );
        }

        UpdateMaterial();
    }

    public void SetFacing( string facing )
    {
        if( facing == "Right" )
            GetComponent<Renderer>().material.SetTextureScale( "_MainTex", new Vector2( 1, 1 ) );
        else if( facing == "Left" )
            GetComponent<Renderer>().material.SetTextureScale( "_MainTex", new Vector2( -1, 1 ) );
    }

    private void UpdateMaterial()
    {
        // Set the material to the current frame of animation.
        GetComponent<Renderer>().material.SetTexture( "_MainTex", texLoop[currentFrame] );
        Color color = GetComponent<Renderer>().material.color;
        color.a = dim ? 1f - DimAmount : 1f;
        GetComponent<Renderer>().material.color = color;
    }

    void Update()
    {
        if( !paused )
        {
            frameTimer += Time.deltaTime;
            if( frameTimer >= frameTime )
            {
                // If we're not looping, run the animation through once and then go back to idle.
                if( !looping )
                {
                    currentFrame++;

                    // Go to idle once we've reached the end of the set.
                    if( currentFrame >= texLoop.Length )
                        SetAnimation( "Idle" );
                }
                // Otherwise, keep going.
                else
                {
                    if( texLoop.Length > 1 )
                    {
                        // We use different math depending on the direction we're going in the loop.
                        if( loopingForward )
                        {
                            currentFrame++;
                            if( currentFrame >= texLoop.Length )
                            {
                                currentFrame -= 2;
                                loopingForward = false;
                            }
                        }
                        else
                        {
                            currentFrame--;
                            if( currentFrame < 0 )
                            {
                                currentFrame += 2;
                                loopingForward = true;
                            }
                        }
                    }

                    // Dim/undim the texture if we're flashing.
                    if( flashing )
                        dim = !dim;
                }

                frameTimer = 0f;
                UpdateMaterial();
            }

            // Fade out
            if( fadingOut )
            {
                fadeOutTimer += Time.deltaTime;
                if( fadeOutTimer < fadeOutTime )
                    DimAmount = fadeOutTimer / fadeOutTime;
                else
                    DimAmount = 1f;
            }
        }
    }
}