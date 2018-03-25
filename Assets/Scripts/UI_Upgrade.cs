using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Upgrade : MonoBehaviour {

	bool itemSet;
	bool upgradePurchased;

	public Item setItem;
	public List<Item.ItemStat> setStats;

	// Use this for initialization
	void Start () {
		itemSet = false;
		upgradePurchased = false;
		setStats = new List<Item.ItemStat> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetItem(Item item)
	{
		setItem = item;
		itemSet = true;
	}

	public void SetStats()
	{
		if (itemSet)
		{
			// Duplicate the all possible stats of the item
			List<Item.ItemStat> statsLeft = new List<Item.ItemStat> (setItem.GetStats ());

			for(int i = 0; i < 3; i++)
			{
				Item.ItemStat thisStat = statsLeft [UnityEngine.Random.Range (0, statsLeft.Count)];
				setStats.Add (thisStat);
				statsLeft.Remove (thisStat);
			}

			upgradePurchased = true;
		}
	}

	public void Upgrade(int index)
	{
		if (itemSet && upgradePurchased)
		{
			Upgrade (setItem, setStats [index]);
		}
	}

	public void Upgrade(Item item, Item.ItemStat stat)
	{
		// Get value
		float value;

		try
		{
			value = (float)item.GetType ().GetField (stat.field).GetValue (item);
		} catch (InvalidCastException e)
		{
			value = (int)item.GetType ().GetField (stat.field).GetValue (item);
		}

		float increment = stat.increment;
		if (stat.increaseOnLv)
			increment *= -1;

		value += increment;

		// Set value
		try
		{
			(float)item.GetType ().GetField (stat.field).SetValue(item, value);
		} catch (InvalidCastException e)
		{
			(int)item.GetType ().GetField (stat.field).SetValue (item, value);
		}
	}
}
