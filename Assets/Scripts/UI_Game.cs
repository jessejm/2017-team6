using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour {

	// External references
	public Sprite mask;
	// UI element references
	RectTransform healthBar;
	Text healthText;
	Text scrapCount;
	Text energyCount;
	Text wireCount;
	Text scoreText;
	Text ammoCount;
	RectTransform reloadMask;
	RectTransform itemsPanel;
	Transform[] inventoryItems;
	FadingPanel waveDisplay;
	RectTransform tooltipPanel;
	UI_Tooltip tooltip;
	// Player component references
	PlayerManager player;
	PlayerUse playerUse;
	Inventory inventory;

	// Inventory Aspects
	bool inventoryOpen;
	float itemSwitchStartTime = -1f;
	int inventorySelectedIndex;

	// Wave Display state
	Text waveText;
	Game.GameState previousState;
	Game game;

	// Use this for initialization
	void Start () {
		healthBar = transform.Find ("HealthPanel/HealthBarBack/HealthBar").GetComponent<RectTransform>();
		healthText = transform.Find ("HealthPanel/HealthBarBack/HealthText").GetComponent<Text>();
		scrapCount = transform.Find ("HealthPanel/MaterialsPanel/ScrapCount").GetComponent<Text>();
		energyCount = transform.Find ("HealthPanel/MaterialsPanel/EnergyCount").GetComponent<Text>();
		wireCount = transform.Find ("HealthPanel/MaterialsPanel/WireCount").GetComponent<Text>();
		itemsPanel = transform.Find ("Items").GetComponent<RectTransform>();
		scoreText = transform.Find ("ScorePanel/Score").GetComponent<Text> ();
		ammoCount = transform.Find ("AmmoCount/AmmoText").GetComponent<Text> ();
		reloadMask = transform.Find ("AmmoCount/ReloadMask").GetComponent<RectTransform> ();

		waveDisplay = GameObject.Find ("WaveDisplay").GetComponent<FadingPanel>();
		waveText = transform.Find ("WaveDisplay/WaveCount").GetComponent<Text> ();
		game = GameObject.Find ("Game").GetComponent<Game>();
		previousState = game.currentState;

		// Find the references for each inventory item panel
		Transform hotbarParent = transform.Find ("Items/Hotbar");
		Transform inventoryParent = transform.Find ("Items/Inventory");
		inventoryItems = new Transform[hotbarParent.childCount + inventoryParent.childCount];
		for (int i = 0; i < inventoryItems.Length; i++) {
			if (i < hotbarParent.childCount) {
				inventoryItems [i] = hotbarParent.GetChild (i);
			}
			else {
				inventoryItems [i] = inventoryParent.GetChild (i - 6);
			}
			UI_Item selectable = inventoryItems [i].GetComponentInChildren<UI_Item>();
			selectable.parent = this;
			selectable.index = i;
		}

		tooltipPanel = transform.Find ("ItemTooltip").GetComponent<RectTransform>();
		tooltip = tooltipPanel.GetComponent<UI_Tooltip> ();
		tooltipPanel.gameObject.SetActive (false);

		GameObject playerObj = GameObject.Find ("Player");
		player = playerObj.GetComponent<PlayerManager>();
		playerUse = playerObj.GetComponent<PlayerUse> ();
		inventory = playerObj.GetComponent<Inventory> ();

		inventoryOpen = false;
		inventorySelectedIndex = -1;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateHealth ();
		UpdateMaterials ();
		UpdateInventory ();
		UpdateInventoryMenu ();
		UpdateScore ();
		UpdateWaveDisplay ();
		UpdateAmmo ();

		// TODO: Update Input
		if (Input.GetKeyDown (KeyCode.F)) {
			if (inventoryOpen)
				CloseInventory ();
			else
				OpenInventory ();
		}
		UpdateItemPanel ();
		// update the game's current state
		previousState = game.currentState;
	}
	
	// Updates the ammo text.  Displays "----" if the current equipped item is not a weapon
	// "Ammo / Clip" if the weapon is not reloading
	// Or how much time is left on the reload if the weapon is reloading
	void UpdateAmmo() {
		Item currentItem = playerUse.currentEquipped;
		if (!(inventory.items[playerUse.selectedIndex] is Weapon)) {
			reloadMask.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (float) 136.0);
			ammoCount.text = "";
			return;
		}
		Weapon currentWeapon = (Weapon)currentItem;
		if (currentWeapon.IsReloading) {
			reloadMask.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (float) 136.0 * currentWeapon.GetReloadPercent());
			// reloadMask.offsetMax = new Vector2 (reloadMask.offsetMax.x, 67 * currentWeapon.GetReloadPercent ());
			ammoCount.text = "-- / --";
		} else {
			reloadMask.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, (float) 136.0);
			ammoCount.text = currentWeapon.Ammo.ToString () + " / " + currentWeapon.ClipSize.ToString ();
		}

	}

	// Checks whether or not the wave number needs to be displayed and updates the wave number
	public void UpdateWaveDisplay()
	{
		Game.GameState currentState = game.currentState;
		bool stateHasChanged = previousState != currentState;
		if (currentState != Game.GameState.END) {
			waveText.text = "WAVE " + game.wave.ToString ();
		}
		if (currentState == Game.GameState.DEFEND && stateHasChanged) {
			waveDisplay.FadeOut ();
		} else if (currentState == Game.GameState.REST && stateHasChanged) {
			waveDisplay.FadeIn ();
		} else if (currentState == Game.GameState.END && stateHasChanged) {
			waveText.text = "GAME OVER";
			waveDisplay.FadeIn ();
		}
	}

	public void UpdateScore()
	{
		scoreText.text = player.score.ToString ();
	}

	public void UpdateHealth() {
		Vector2 scale = healthBar.sizeDelta;
		scale.x = ((float)player.currentHealth / (float)player.maxHealth) * 200;
		healthBar.sizeDelta = scale;
		//Set hp text to 0 if negative
		healthText.text = Mathf.Max(0, player.currentHealth).ToString ();
	}

	public void UpdateMaterials() {
		scrapCount.text = inventory.resources.scrap.ToString();
		energyCount.text = inventory.resources.energy.ToString();
		wireCount.text = inventory.resources.wire.ToString();
	}

	public void UpdateInventory() {
		for(int i = 0; i < 24; i++) {
			Transform imageTrans;
			// Check if image exists, it may not if following the mouse
			if (imageTrans = inventoryItems [i].Find ("Image"))
			{
				Image image = imageTrans.GetComponent<Image> ();
				if (i < inventory.items.Count && inventory.items [i] != null) {
					//Set the ui image to that of the hotbar item
					image.sprite = inventory.items [i].itemImg;
				} else {
					//Set the ui image to a clear image
					image.sprite = mask;
				}
			}
		}
		//Highlight the square of the selected item
		for (int i = 0; i < 24; i++) {
			Image box = inventoryItems[i].GetComponent<Image>();
			if (player.SelectedIndex() == i) {
				//Make the selected box yellow
				box.color = new Color(1f, 0.765f, 0);
			} else {
				//Make everything else white
				box.color = Color.white;
			}
		}
	}

	void UpdateItemPanel() {
		float progress = (Time.fixedTime - itemSwitchStartTime) * 4;
		if (inventoryOpen) {
			itemsPanel.anchoredPosition = Vector2.Lerp (Vector2.zero, Vector2.up * 188f, progress);
		} else {
			itemsPanel.anchoredPosition = Vector2.Lerp (Vector2.up * 188f, Vector2.zero, progress);
		}
	}

	public void OpenInventory() {
		inventoryOpen = true;
		itemSwitchStartTime = Time.fixedTime;
		player.GetComponent<PlayerUse> ().enabled = false;
		// TODO: Play sound
	}

	public void CloseInventory() {
		inventoryOpen = false;
		itemSwitchStartTime = Time.fixedTime;
		player.GetComponent<PlayerUse> ().enabled = true;
		playerUse.updateSelected ();
		// TODO: Play sound
	}

	void UpdateInventoryMenu()
	{
		if (inventorySelectedIndex >= 0)
		{
			if (inventoryOpen)
			{
				// Have the selected item's image follow the mouse
				itemsPanel.Find ("Image").position = Input.mousePosition;
			}
			else
			{
				// Reset the selected item
				Transform image = itemsPanel.Find("Image");
				image.SetParent(inventoryItems [inventorySelectedIndex]);
				image.localPosition = Vector3.zero;
				inventorySelectedIndex = -1;
			}
		}
		else if (tooltipPanel.gameObject.activeSelf)
		{
			tooltipPanel.position = Input.mousePosition;
		}
	}

	public void MouseEnter(int index)
	{
		Item thisItem = inventory.items [index];

		if (inventoryOpen && inventorySelectedIndex < 0 && thisItem != null)
		{
			if (!tooltipPanel.gameObject.activeSelf)
			{
				tooltipPanel.gameObject.SetActive (true);
			}

			tooltip.icon.sprite = thisItem.itemImg;
			tooltip.name.text = thisItem.itemName;

			// TODO: Print levels and tiers

			List<Item.ItemStat> stats = thisItem.GetStats ();
			// Resize panel to fit each stat
			float toolTipWidth = tooltipPanel.sizeDelta.x;
			float additionalSpace = Mathf.Max (0f, (stats.Count - 2) * 15f);
			tooltipPanel.sizeDelta = new Vector2 (toolTipWidth, 72f + additionalSpace);
			// Generate appropriate number of text views
			tooltip.generateTextViews (stats.Count);
			// Fill in stat data
			for (int i = 0; i < stats.Count; i++)
			{
				Item.ItemStat thisStat = stats [i];
				Text statText = tooltip.stats [i];
				// Set name
				statText.text = thisStat.name;
				// Get and set value
				float value;

				try
				{
					value = (float)thisItem.GetType ().GetField (thisStat.field).GetValue (thisItem);
				} catch (InvalidCastException e)
				{
					value = (int)thisItem.GetType ().GetField (thisStat.field).GetValue (thisItem);
				}

				statText.transform.Find ("Value").GetComponent<Text> ().text = value.ToString ();
			}
		}
	}

	public void MouseExit()
	{
		tooltipPanel.gameObject.SetActive (false);
	}

	public void MouseLeftClick(int index)
	{
		if (inventoryOpen)
		{
			if (inventorySelectedIndex >= 0)
			{
				itemsPanel.Find ("Image").SetParent(inventoryItems [inventorySelectedIndex]);

				// Swap the two items
				Item temp = inventory.items [index];
				inventory.items [index] = inventory.items [inventorySelectedIndex];
				inventory.items [inventorySelectedIndex] = temp;

				inventoryItems [inventorySelectedIndex].Find ("Image").localPosition = Vector3.zero;
				inventorySelectedIndex = -1;
			}
			else
			{
				// Pick up the selected item, but not if its an empty space
				if (inventory.items [index] != null)
				{
					inventorySelectedIndex = index;
					inventoryItems [inventorySelectedIndex].Find ("Image").SetParent (itemsPanel);
					tooltipPanel.gameObject.SetActive (false);
				}
			}
		}
	}

	public void MouseRightClick(int index)
	{
		// TODO Give context menu options to drop or upgrade?
	}
}
