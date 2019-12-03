using Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form

    {
        private Player _player;
        //Monster variable that the player is fighting at the current location
        private Monster _currentMonster;
        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10,10,20,0,1);
            //"Move" player to their home, default location essentially
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            //Start player off with a rusty sword
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));


            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();

        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

        private void BtnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void BtnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void BtnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void BtnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }



        private void MoveTo(Location newLocation)
        {
            //Does location have any required items?
            if(newLocation.ItemRequiredToEnter != null)
            {
                //Check to see if player has the required item in inventory.
                bool playerHasRequiredItem = false;

                foreach (InventoryItem item in _player.Inventory)
                {
                    if(item.Details.ID == newLocation.ItemRequiredToEnter.ID)
                    {
                        playerHasRequiredItem = true;
                        break;
                    }
                }

                if (!playerHasRequiredItem)
                {
                    //Did not find the required item in their inventory
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                    return;
                }
            }

            //Update the player's current location
            _player.CurrentLocation = newLocation;

            // Show/hide available movement buttons.  If no location is present, it will be not visible
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            // Display current location name and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            // Completely heal the player (hitpoints restored when going to new location.)
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //Update Hit Points in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();


            //See if the player has the quest and if they have completed it
            bool playerAlreadyHasQuest = false;
            bool playerAlreadyCompletedQuest = false;
            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            { 

                foreach (PlayerQuest playerQuest in _player.Quests)
                {
                    if(playerQuest.Details.ID == newLocation.QuestAvailableHere.ID)
                    {
                        playerAlreadyHasQuest = true;

                        if (playerQuest.IsCompleted)
                        {
                            playerAlreadyCompletedQuest = true;
                        }
                    }
                }

            }

            //See if the player has the quest
            if (playerAlreadyHasQuest)
            {
                //If the player has not completd the quest yet
                if (!playerAlreadyCompletedQuest)
                {
                    //See if the player has all the items needed to complete the quest
                    bool playerHasAllItemsToCompleteQuest = true;

                    foreach (QuestCompletionItem item in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        bool foundItemInPlayersInventory = false;

                        //Check each item in the player's inventory, to see if they have it and enough of it
                        foreach (InventoryItem item2 in _player.Inventory)
                        {
                            //The player has this item in their inventory
                            if (item2.Details.ID == item.Details.ID)
                            {
                                foundItemInPlayersInventory = true;

                                if (item2.Quantity < item.Quantity)
                                {
                                    //The player does not have enough of this item
                                    playerHasAllItemsToCompleteQuest = false;

                                    break;
                                }
                                break;
                            }
                        }
                        //Did not find required item, set our variable and stop looking
                        if (!foundItemInPlayersInventory)
                        {
                            //The player does not have this item in their inventory
                            playerHasAllItemsToCompleteQuest = false;

                            //No reason to continue checking for the other quest completion items
                            break;
                        }
                    }

                    //Player has all items required to complete the quest.
                    if (playerHasAllItemsToCompleteQuest)
                    {
                        //Display message
                        rtbMessages.Text += Environment.NewLine;
                        rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest."
                            + Environment.NewLine;

                        //Remove quest items from inventory
                        foreach (QuestCompletionItem item in newLocation.QuestAvailableHere.QuestCompletionItems)
                        {
                            foreach (InventoryItem item2 in _player.Inventory)
                            {
                                if (item2.Details.ID == item.Details.ID)
                                {
                                    //Subtract the quantity from the player's inventory that was needed to complete the quest
                                    item2.Quantity -= item.Quantity;
                                    break;
                                }
                            }
                        }

                        //Give quest rewards
                        rtbMessages.Text += "You receive: " + Environment.NewLine;
                        rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() +
                            " experience points" + Environment.NewLine;
                        rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() +
                            " gold." + Environment.NewLine;
                        rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                        rtbMessages.Text += Environment.NewLine;

                        _player.ExperiencePoints +=
                            newLocation.QuestAvailableHere.RewardExperiencePoints;
                        _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                        //Add the reward item to the player's inventory
                        bool addedItemToPlayerInventory = false;

                        foreach (InventoryItem item in _player.Inventory)
                        {
                            if (item.Details.ID ==
                                newLocation.QuestAvailableHere.RewardItem.ID)
                            {
                                //Have the item inventory, increase by one
                                item.Quantity++;

                                addedItemToPlayerInventory = true;

                                break;
                            }
                        }

                        //Didn't have the item, adds to inventory with a qty of 1
                        if (!addedItemToPlayerInventory)
                        {
                            _player.Inventory.Add(new InventoryItem(newLocation.QuestAvailableHere.RewardItem, 1));
                        }

                        //Mark the quest completed
                        //Find the quest in the player's quest list
                        foreach (PlayerQuest quest in _player.Quests)
                        {
                            if (quest.Details.ID == newLocation.QuestAvailableHere.ID)
                            {
                                //Mark it as completed
                                quest.IsCompleted = true;
                                break;
                            }
                        }

                    }
                }
            }
            else
            {
                //The player does not have quest

                //Display the messages
                rtbMessages.Text += "You receive the " +
                    newLocation.QuestAvailableHere.Name +
                    " quest." + Environment.NewLine;
                rtbMessages.Text += newLocation.QuestAvailableHere.Description +
                    Environment.NewLine;
                rtbMessages.Text += "To complete it, return with:" +
                    Environment.NewLine;
                foreach (QuestCompletionItem item in newLocation.QuestAvailableHere.QuestCompletionItems)
                {
                    if (item.Quantity == 1)
                    {
                        rtbMessages.Text += item.Quantity.ToString() + " " +
                            item.Details.NamePlural + Environment.NewLine;
                    }
                }
                rtbMessages.Text += Environment.NewLine;
                //Add the quest to the player's quest list
                _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
            }
            //Does the location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name +
                    Environment.NewLine;

                //Make a new monster, using the values from the standard monster in the
                // World.Monster list
                Monster standardMonster = World.MonsterByID(
                    newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage,
                    standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints,
                     standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);

                }
                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;

            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            //Refresh player's inventory list
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();
            
            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { inventoryItem.Details.Name,
                    inventoryItem.Quantity.ToString()});

                }
            }

            //Refresh player's quest list

            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name,
                 playerQuest.IsCompleted.ToString()});
            }

            //Refresh player's weapons combobox
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is Weapon)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if(weapons.Count == 0)
            {
                // The player doesn't have any weapons, hide the cbo and useweapon btn
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;

            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }

            //Refresh player's potions combo box
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is HealingPotion)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details);
                    }
                }
            }

            if(healingPotions.Count == 0)
            {
                //Player has no potions left, hide the potion combo box and use button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
            
        }

        private void BtnUseWeapon_Click(object sender, EventArgs e)
        {

        }

        private void BtnUsePotion_Click(object sender, EventArgs e)
        {

        }




    }
}
