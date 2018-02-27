﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace vMenuClient
{
    public class VehicleSpawner
    {
        // Variables
        private UIMenu menu;
        private Notification Notify = MainMenu.Notify;
        private Subtitles Subtitle = MainMenu.Subtitle;
        private CommonFunctions cf = MainMenu.cf;

        public bool SpawnInVehicle { get; private set; } = true;
        public bool ReplaceVehicle { get; private set; } = true;

        private void CreateMenu()
        {
            VehicleData vd = new VehicleData();

            // Create the menu.
            menu = new UIMenu(GetPlayerName(PlayerId()), "Vehicle Spawner")
            {
                MouseEdgeEnabled = false
            };

            // Create the buttons and checkboxes.
            UIMenuItem spawnByName = new UIMenuItem("Spawn By Name", "Enter the name of a vehicle to spawn.");
            UIMenuCheckboxItem spawnInVeh = new UIMenuCheckboxItem("Spawn Inside Vehicle", SpawnInVehicle, "This will teleport you into the vehicle when you spawn it.");
            UIMenuCheckboxItem replacePrev = new UIMenuCheckboxItem("Replace Previous", SpawnInVehicle, "This will automatically delete your previously spawned vehicle when you spawn a new vehicle.");

            // Add the items to the menu.
            menu.AddItem(spawnByName);
            menu.AddItem(spawnInVeh);
            menu.AddItem(replacePrev);

            // Create the submenus for each category.


            var vl = new Vehicles();

            // Loop through all the vehicle classes.
            for (var vehClass = 0; vehClass < 22; vehClass++)
            {
                // Get the class name.
                string className = cf.GetLocalizedName($"VEH_CLASS_{vehClass.ToString()}");

                // Create a button & a menu for it, add the menu to the menu pool and add & bind the button to the menu.
                UIMenuItem btn = new UIMenuItem(className, $"Select a vehicle from the {className} class.");
                UIMenu vehicleClassMenu = new UIMenu("Vehicle Spawner", className);
                MainMenu.Mp.Add(vehicleClassMenu);
                menu.AddItem(btn);
                menu.BindMenuToItem(vehicleClassMenu, btn);

                // Create a dictionary for the duplicate vehicle names (in this vehicle class).
                var duplicateVehNames = new Dictionary<string, int>();

                // Loop through all the vehicles in the vehicle class.
                foreach (var veh in vl.VehicleClasses[className])
                {

                    // Convert the model name to start with a Capital letter, converting the other characters to lowercase. 
                    var properCasedModelName = veh[0].ToString().ToUpper() + veh.ToLower().Substring(1);

                    // Get the localized vehicle name, if it's "NULL" (no label found) then use the "properCasedModelName" created above.
                    var vehName = cf.GetVehDisplayNameFromModel(veh) != "NULL" ? cf.GetVehDisplayNameFromModel(veh) : properCasedModelName;

                    // Loop through all the menu items and check each item's title/text and see if it matches the current vehicle (display) name.
                    var duplicate = false;
                    for (var itemIndex = 0; itemIndex < vehicleClassMenu.MenuItems.Count; itemIndex++)
                    {
                        // If it matches...
                        if (vehicleClassMenu.MenuItems[itemIndex].Text.ToString() == vehName)
                        {

                            // Check if the model was marked as duplicate before.
                            if (duplicateVehNames.Keys.Contains(vehName))
                            {
                                // If so, add 1 to the duplicate counter for this model name.
                                duplicateVehNames[vehName]++;
                            }

                            // If this is the first duplicate, then set it to 2.
                            else
                            {
                                duplicateVehNames[vehName] = 2;
                            }

                            // The model name is a duplicate, so get the modelname and add the duplicate amount for this model name to the end of the vehicle name.
                            vehName += $" ({duplicateVehNames[vehName].ToString()})";

                            // Then create and add a new button for this vehicle.

                            if (cf.DoesModelExist(veh))
                            {
                                var vehBtn = new UIMenuItem(vehName) { Enabled = true };
                                vehicleClassMenu.AddItem(vehBtn);
                            }
                            else
                            {
                                var vehBtn = new UIMenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false };
                                vehicleClassMenu.AddItem(vehBtn);
                                vehBtn.SetRightBadge(UIMenuItem.BadgeStyle.Lock);
                            }
                            //vehicleClassMenu.AddItem(new UIMenuItem(vehName));

                            // Mark duplicate as true and break from the loop because we already found the duplicate.
                            duplicate = true;
                            break;
                        }
                    }

                    // If it's not a duplicate, add the model name.
                    if (!duplicate)
                    {
                        if (cf.DoesModelExist(veh))
                        {
                            var vehBtn = new UIMenuItem(vehName) { Enabled = true };
                            vehicleClassMenu.AddItem(vehBtn);
                        }
                        else
                        {
                            var vehBtn = new UIMenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false };
                            vehicleClassMenu.AddItem(vehBtn);
                            vehBtn.SetRightBadge(UIMenuItem.BadgeStyle.Lock);
                        }
                    }
                }

                vehicleClassMenu.OnItemSelect += (sender2, item2, index2) =>
                {
                    cf.SpawnVehicle(vl.VehicleClasses[className][index2], SpawnInVehicle, ReplaceVehicle);
                };

            }


            // Handle button presses.
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == spawnByName)
                {
                    // Passing "custom" as the vehicle name, will ask the user for input.
                    cf.SpawnVehicle("custom");
                }
            };

            // Handle checkbox changes.
            menu.OnCheckboxChange += (sender, item, _checked) =>
            {
                if (item == spawnInVeh)
                {
                    SpawnInVehicle = _checked;
                }
                else if (item == replacePrev)
                {
                    ReplaceVehicle = _checked;
                }
            };
        }

        /// <summary>
        /// Create the menu if it doesn't exist, and then returns it.
        /// </summary>
        /// <returns>The Menu</returns>
        public UIMenu GetMenu()
        {
            if (menu == null)
            {
                CreateMenu();
            }
            return menu;
        }
    }
}