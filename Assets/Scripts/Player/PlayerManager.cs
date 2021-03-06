using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class used to manage the player
public class PlayerManager : UnitManager
{
	public int score;

    // public Slider healthSlider;

    public float flashSpeed;
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f);

    AudioSource playerAudio;
    PlayerMovement playerMovement;
    PlayerUse playerUse;
    PlayerSound playerSound;
    Renderer playerRenderer;

    Color originalColor;

    // Use this for initialization
    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

		score = 0;

        playerAudio = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerUse = GetComponent<PlayerUse>();
        // playerSound = GetComponent<PlayerSound>();
        playerRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        originalColor = playerRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (damaged)
        {
            playerRenderer.material.color = flashColor;
        }
        else
        {
            // Transition the color back to normal
            playerRenderer.material.color = Color.Lerp(playerRenderer.material.color, originalColor, flashSpeed * Time.deltaTime);
        }

        // Reset the damage
        damaged = false;
    }

    public override void applyDamage(int damage)
    {
        // Set damage to be true so we can show it
        damaged = true;

        // Apply the damage
        currentHealth = currentHealth - damage;

        // Play player hit sound
        // playerSound.playHitSound();

        // Check if the player is dead
        if (currentHealth <= 0 && !isDead)
        {
            Death();
        }
    }

	// Add points to the player's score
	public void AddPoints(int points)
	{
		score += points;
	}

	public int SelectedIndex()
	{
		return playerUse.selectedIndex;
	}


	protected override void Death()
    {
        isDead = true;

        // playerUse.DisableEffects();
        // anim.SetTrigger("Die");

        // playerAudio.clip = deathClip;
        // playerAudio.Play();

        playerMovement.enabled = false;
        playerUse.enabled = false;
    }

	public override void removeHeldItem()
	{
		playerUse.removeHeldItem ();
	}
}
