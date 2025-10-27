/**
 *  File           Form1.cs
 *  Brief          Metal Slug X (GOG Version) PC Trainer
 *  Copyright      2025 Shawn M. Crawford [sleepy]
 *  Date           10/08/2025
 *
 *  Author         Shawn M. Crawford [sleepy]
 *
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MetalSlugXTrainer
{
    public partial class Form1 : Form
    {
        #region Imports

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

        #endregion

        #region Enums

        [Flags]
        private enum ProcessAccessRights
        {
            PROCESS_CREATE_PROCESS            = 0x0080,
            PROCESS_CREATE_THREAD             = 0x0002,
            PROCESS_DUP_HANDLE                = 0x0040,
            PROCESS_QUERY_INFORMATION         = 0x0400,
            PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
            PROCESS_SET_INFORMATION           = 0x0200,
            PROCESS_SET_QUOTA                 = 0x0100,
            PROCESS_SUSPEND_RESUME            = 0x0800,
            PROCESS_TERMINATE                 = 0x0001,
            PROCESS_VM_OPERATION              = 0x0008,
            PROCESS_VM_READ                   = 0x0010,
            PROCESS_VM_WRITE                  = 0x0020,
            DELETE                            = 0x00010000,
            READ_CONTROL                      = 0x00020000,
            SYNCHRONIZE                       = 0x00100000,
            WRITE_DAC                         = 0x00040000,
            WRITE_OWNER                       = 0x00080000,
            STANDARD_RIGHTS_REQUIRED          = 0x000f0000,
            PROCESS_ALL_ACCESS                = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF)
        }

        #endregion

        #region Constants

        private const string MODULE_NAME = "mslugx.exe";
        private const string PROCESS_NAME = "mslugx";

        private const int BASE_ADDRESS_1 = 0x000F7BB8; // Used for Player 1 and everything else
        private const int BASE_ADDRESS_2 = 0x000F7BBC; // Used for Player 2 and everything else (alt)
        private const int BASE_ADDRESS_3 = 0x000F7BC0; // Used for credits

        private const int OFFSET_LEVEL_TIMER = 0x13DF;
        private const int OFFSET_CONTINUE_TIMER = 0x13E0; //13E1, 

        // Player 1
        private const int OFFSET_P1_CREDITS_COUNT = 0x34;
        private const int OFFSET_P1_LIVES_COUNT = 0x1289;
        private const int OFFSET_P1_LIVES_COUNT_COMPLETE = 0x128B; // Set this offset value to 0 after setting lives.
        private const int OFFSET_P1_INVINCIBILITY_TIMER = 0x1548;
        //private const int OFFSET_P1_STATUS = 0x1554; // (3, 7, 11 = Mummy, character animations, 2 bytes? ignore...)
        private const int OFFSET_P1_SCORE = 0xC46A; // Score Stored as Hex?
        private const int OFFSET_P1_POWS_RESCUED = 0xC5FC;
        private const int OFFSET_P1_VEHICLE_HEALTH_COUNT = 0xC620; // Max 48
        private const int OFFSET_P1_VEHICLE_AMMO_CANON_COUNT = 0xC626;
        private const int OFFSET_P1_WEAPON_TYPE = 0xC71F;
        private const int OFFSET_P1_BOMB_COUNT = 0xC721;
        private const int OFFSET_P1_BOMB_TYPE = 0xC723; // (0 = None, Grenade, Fire Bomb, Stone)
        private const int OFFSET_P1_AMMO_COUNT = 0xC724;
        private const int OFFSET_P1_CURRENT_CHARACTER = 0x1532;

        // Player 2
        private const int OFFSET_P2_CREDITS_COUNT = 0x35;
        private const int OFFSET_P2_LIVES_COUNT = 0x1339;
        private const int OFFSET_P2_LIVES_COUNT_COMPLETE = 0x133B; // Set this offset value to 0 after setting lives.
        private const int OFFSET_P2_INVINCIBILITY_TIMER = 0x15F8;
        //private const int OFFSET_P2_STATUS = 0x1606; // (3, 7, 11 = Mummy, character animations, 2 bytes? ignore...)
        private const int OFFSET_P2_SCORE = 0xC472; // Score Stored as Hex?
        private const int OFFSET_P2_POWS_RESCUED = 0xC600;
        private const int OFFSET_P2_VEHICLE_HEALTH_COUNT = 0xC644; // Max 48
        private const int OFFSET_P2_VEHICLE_AMMO_CANON_COUNT = 0xC64A;
        private const int OFFSET_P2_WEAPON_TYPE = 0xC71E;
        private const int OFFSET_P2_BOMB_COUNT = 0xC720;
        private const int OFFSET_P2_BOMB_TYPE = 0xC722; // (0 = None, Grenade, Fire Bomb, Stone)
        private const int OFFSET_P2_AMMO_COUNT = 0xC726;
        private const int OFFSET_P2_CURRENT_CHARACTER = 0x15E2;

        #endregion

        #region Fields

        private Process game;

        private IntPtr hProc = IntPtr.Zero;
        private IntPtr levelTimerAddressGlobal = IntPtr.Zero;
        private IntPtr continueTimerAddressGlobal = IntPtr.Zero;
        private IntPtr livesCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr livesCountP1CompleteAddressGlobal = IntPtr.Zero;
        private IntPtr livesCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr livesCountP2CompleteAddressGlobal = IntPtr.Zero;
        private IntPtr invincibilityTimerP1AddressGlobal = IntPtr.Zero;
        private IntPtr invincibilityTimerP2AddressGlobal = IntPtr.Zero;
        private IntPtr powsRescuedP1AddressGlobal = IntPtr.Zero;
        private IntPtr powsRescuedP2AddressGlobal = IntPtr.Zero;
        private IntPtr weaponTypeP1AddressGlobal = IntPtr.Zero;
        private IntPtr weaponTypeP2AddressGlobal = IntPtr.Zero;
        private IntPtr bombTypeP1AddressGlobal = IntPtr.Zero;
        private IntPtr bombTypeP2AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr bombCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr ammoCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr ammoCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleAmmoCanonCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleAmmoCanonCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleHealthCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr vehicleHealthCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr creditsCountP1AddressGlobal = IntPtr.Zero;
        private IntPtr creditsCountP2AddressGlobal = IntPtr.Zero;
        private IntPtr currentCharacterP1AddressGlobal = IntPtr.Zero;
        private IntPtr currentCharacterP2AddressGlobal = IntPtr.Zero;
        private IntPtr scoreP1AddressGlobal = IntPtr.Zero;
        private IntPtr scoreP2AddressGlobal = IntPtr.Zero;

        private Timer timer = new Timer();

        private string previousLogEntry = "";

        #endregion

        #region Methods

        public Form1()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            StartPosition = FormStartPosition.CenterScreen;

            textBoxLog.Text = "Metal Slug X (GOG Version) Trainer by sLeEpY9090" + Environment.NewLine;

            PopulateWeaponTypes();
            PopulateBombTypes();
            PopulateCharacters();
            PopulateScore();
            SetTextBoxMaxLength();
            SetDefaultTextBoxValues();

            // Keep trainer updated about the game process and game memory.
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        #region Form Setup Methods

        private void PopulateWeaponTypes()
        {
            Dictionary<int, string> weaponTypesDictionary = new Dictionary<int, string>
            {
                {  0, " 0 - Hand Gun"                },
                {  1, " 1 - Cannon"                  },
                {  2, " 2 - Shotgun"                 },
                {  3, " 3 - Rocket Launcher"         },
                {  4, " 4 - Flame Shot"              },
                {  5, " 5 - Heavy Machine Gun"       },
                {  6, " 6 - Laser Gun"               },
                {  7, " 7 - Super Shotgun"           },
                {  8, " 8 - Super Rocket Launcher"   },
                {  9, " 9 - Super Flame Shot"        },
                { 10, "10 - Super Heavy Machine Gun" },
                { 11, "11 - Super Laser Gun"         },
                { 12, "12 - Enemy Chaser"            },
                { 13, "13 - Iron Lizard"             },
                { 14, "14 - Dropshot"                },
                { 15, "15 - Super Cannon"            }
            };

            comboBoxP1WeaponTypeRead.Items.Clear();
            comboBoxP1WeaponTypeRead.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP1WeaponTypeRead.DisplayMember = "Value";
            comboBoxP1WeaponTypeRead.ValueMember = "Key";
            comboBoxP1WeaponTypeRead.SelectedIndex = 0;

            comboBoxP2WeaponTypeRead.Items.Clear();
            comboBoxP2WeaponTypeRead.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP2WeaponTypeRead.DisplayMember = "Value";
            comboBoxP2WeaponTypeRead.ValueMember = "Key";
            comboBoxP2WeaponTypeRead.SelectedIndex = 0;

            comboBoxP1WeaponTypeWrite.Items.Clear();
            comboBoxP1WeaponTypeWrite.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP1WeaponTypeWrite.DisplayMember = "Value";
            comboBoxP1WeaponTypeWrite.ValueMember = "Key";
            comboBoxP1WeaponTypeWrite.SelectedIndex = 0;

            comboBoxP2WeaponTypeWrite.Items.Clear();
            comboBoxP2WeaponTypeWrite.DataSource = new BindingSource(weaponTypesDictionary, null);
            comboBoxP2WeaponTypeWrite.DisplayMember = "Value";
            comboBoxP2WeaponTypeWrite.ValueMember = "Key";
            comboBoxP2WeaponTypeWrite.SelectedIndex = 0;
        }

        private void PopulateBombTypes()
        {
            Dictionary<int, string> bombTypesDictionary = new Dictionary<int, string>
            {
                { 0, "0 - None"      },
                { 1, "1 - Grenade"   },
                { 2, "2 - Fire Bomb" },
                { 3, "3 - Stone"     }
            };

            comboBoxP1BombTypeRead.Items.Clear();
            comboBoxP1BombTypeRead.DataSource = new BindingSource(bombTypesDictionary, null);
            comboBoxP1BombTypeRead.DisplayMember = "Value";
            comboBoxP1BombTypeRead.ValueMember = "Key";
            comboBoxP1BombTypeRead.SelectedIndex = 0;

            comboBoxP2BombTypeRead.Items.Clear();
            comboBoxP2BombTypeRead.DataSource = new BindingSource(bombTypesDictionary, null);
            comboBoxP2BombTypeRead.DisplayMember = "Value";
            comboBoxP2BombTypeRead.ValueMember = "Key";
            comboBoxP2BombTypeRead.SelectedIndex = 0;

            comboBoxP1BombTypeWrite.Items.Clear();
            comboBoxP1BombTypeWrite.DataSource = new BindingSource(bombTypesDictionary, null);
            comboBoxP1BombTypeWrite.DisplayMember = "Value";
            comboBoxP1BombTypeWrite.ValueMember = "Key";
            comboBoxP1BombTypeWrite.SelectedIndex = 0;

            comboBoxP2BombTypeWrite.Items.Clear();
            comboBoxP2BombTypeWrite.DataSource = new BindingSource(bombTypesDictionary, null);
            comboBoxP2BombTypeWrite.DisplayMember = "Value";
            comboBoxP2BombTypeWrite.ValueMember = "Key";
            comboBoxP2BombTypeWrite.SelectedIndex = 0;
        }

        private void PopulateCharacters()
        {
            
            Dictionary<int, string> charactersDictionary = new Dictionary<int, string>
            {
                {  0, " 0 - Marco Rossi"   },
                {  1, " 1 - Tarma Roving"  },
                {  2, " 2 - Eri Kasamoto"  },
                {  3, " 3 - Fiolina Germi" },
                {  4, " 4 - Marco Rossi - Special Weapon"   },
                {  5, " 5 - Tarma Roving - Special Weapon"  },
                {  6, " 6 - Eri Kasamoto - Special Weapon"  },
                {  7, " 7 - Fiolina Germi - Special Weapon" },
                {  8, " 8 - Marco Rossi - Fat"   },
                {  9, " 9 - Tarma Roving - Fat"  },
                { 10, "10 - Eri Kasamoto - Fat"  },
                { 11, "11 - Fiolina Germi - Fat" }
            };

            comboBoxP1CurrentCharacterRead.Items.Clear();
            comboBoxP1CurrentCharacterRead.DataSource = new BindingSource(charactersDictionary, null);
            comboBoxP1CurrentCharacterRead.DisplayMember = "Value";
            comboBoxP1CurrentCharacterRead.ValueMember = "Key";
            comboBoxP1CurrentCharacterRead.SelectedIndex = 0;

            comboBoxP1CurrentCharacterWrite.Items.Clear();
            comboBoxP1CurrentCharacterWrite.DataSource = new BindingSource(charactersDictionary, null);
            comboBoxP1CurrentCharacterWrite.DisplayMember = "Value";
            comboBoxP1CurrentCharacterWrite.ValueMember = "Key";
            comboBoxP1CurrentCharacterWrite.SelectedIndex = 0;

            comboBoxP2CurrentCharacterRead.Items.Clear();
            comboBoxP2CurrentCharacterRead.DataSource = new BindingSource(charactersDictionary, null);
            comboBoxP2CurrentCharacterRead.DisplayMember = "Value";
            comboBoxP2CurrentCharacterRead.ValueMember = "Key";
            comboBoxP2CurrentCharacterRead.SelectedIndex = 0;

            comboBoxP2CurrentCharacterWrite.Items.Clear();
            comboBoxP2CurrentCharacterWrite.DataSource = new BindingSource(charactersDictionary, null);
            comboBoxP2CurrentCharacterWrite.DisplayMember = "Value";
            comboBoxP2CurrentCharacterWrite.ValueMember = "Key";
            comboBoxP2CurrentCharacterWrite.SelectedIndex = 0;
        }

        private void PopulateScore()
        {
            for (int i = 0; i < 256; i++)
            {
                comboBoxP1Score1.Items.Add(i.ToString("X"));
                comboBoxP1Score2.Items.Add(i.ToString("X"));
                comboBoxP1Score3.Items.Add(i.ToString("X"));
                comboBoxP1Score4.Items.Add(i.ToString("X"));

                comboBoxP2Score1.Items.Add(i.ToString("X"));
                comboBoxP2Score2.Items.Add(i.ToString("X"));
                comboBoxP2Score3.Items.Add(i.ToString("X"));
                comboBoxP2Score4.Items.Add(i.ToString("X"));
            }

            comboBoxP1Score1.SelectedIndex = 255;
            comboBoxP1Score2.SelectedIndex = 255;
            comboBoxP1Score3.SelectedIndex = 255;
            comboBoxP1Score4.SelectedIndex = 255;

            comboBoxP2Score1.SelectedIndex = 255;
            comboBoxP2Score2.SelectedIndex = 255;
            comboBoxP2Score3.SelectedIndex = 255;
            comboBoxP2Score4.SelectedIndex = 255;
        }

        public void SetDefaultTextBoxValues()
        {
            textBoxLevelTimerWrite.Text = "255";
            textBoxContinueTimerWrite.Text = "255";

            textBoxP1LivesCountWrite.Text = "255";
            textBoxP1InvincibilityTimerWrite.Text = "60"; // Invincible
            textBoxP1POWsRescuedWrite.Text = "255";
            textBoxP1BombCountWrite.Text = "255";
            textBoxP1AmmoCountWrite.Text = "999"; // "65535";
            textBoxP1VehicleAmmoCountWrite.Text = "255";
            textBoxP1VehicleHealthCountWrite.Text = "48"; // Max Vehicle Health
            textBoxP1CreditsCountWrite.Text = "255";

            textBoxP2LivesCountWrite.Text = "255";
            textBoxP2InvincibilityTimerWrite.Text = "60"; // Invincible
            textBoxP2POWsRescuedWrite.Text = "255";
            textBoxP2BombCountWrite.Text = "255";
            textBoxP2AmmoCountWrite.Text = "999"; // "65535";
            textBoxP2VehicleAmmoCountWrite.Text = "255";
            textBoxP2VehicleHealthCountWrite.Text = "48"; // Max Vehicle Health
            textBoxP2CreditsCountWrite.Text = "255";

            textBoxLevelTimerRead.Enabled = false;
            textBoxContinueTimerRead.Enabled = false;

            textBoxP1LivesCountRead.Enabled = false;
            textBoxP1InvincibilityTimerRead.Enabled = false;
            textBoxP1POWsRescuedRead.Enabled = false;
            textBoxP1BombCountRead.Enabled = false;
            textBoxP1AmmoCountRead.Enabled = false;
            textBoxP1VehicleAmmoCountRead.Enabled = false;
            textBoxP1VehicleHealthCountRead.Enabled = false;
            textBoxP1CreditsCountRead.Enabled = false;
            textBoxP1ScoreRead.Enabled = false;
            comboBoxP1BombTypeRead.Enabled = false;
            comboBoxP1WeaponTypeRead.Enabled = false;
            comboBoxP1CurrentCharacterRead.Enabled = false;

            textBoxP2LivesCountRead.Enabled = false;
            textBoxP2InvincibilityTimerRead.Enabled = false;
            textBoxP2POWsRescuedRead.Enabled = false;
            textBoxP2BombCountRead.Enabled = false;
            textBoxP2AmmoCountRead.Enabled = false;
            textBoxP2VehicleAmmoCountRead.Enabled = false;
            textBoxP2VehicleHealthCountRead.Enabled = false;
            textBoxP2CreditsCountRead.Enabled = false;
            textBoxP2ScoreRead.Enabled = false;
            comboBoxP2BombTypeRead.Enabled = false;
            comboBoxP2WeaponTypeRead.Enabled = false;
            comboBoxP2CurrentCharacterRead.Enabled = false;

            textBoxModuleName.Text = MODULE_NAME;
            textBoxProcessName.Text = PROCESS_NAME;
        }

        private void SetTextBoxMaxLength()
        {
            textBoxLevelTimerWrite.MaxLength = 3;
            textBoxContinueTimerWrite.MaxLength = 3;

            textBoxP1LivesCountWrite.MaxLength = 3;
            textBoxP1InvincibilityTimerWrite.MaxLength = 3;
            textBoxP1POWsRescuedWrite.MaxLength = 3;
            textBoxP1BombCountWrite.MaxLength = 3;
            textBoxP1AmmoCountWrite.MaxLength = 5;
            textBoxP1VehicleAmmoCountWrite.MaxLength = 3;
            textBoxP1VehicleHealthCountWrite.MaxLength = 3;
            textBoxP1CreditsCountWrite.MaxLength = 3;

            textBoxP2LivesCountWrite.MaxLength = 3;
            textBoxP2InvincibilityTimerWrite.MaxLength = 3;
            textBoxP2POWsRescuedWrite.MaxLength = 3;
            textBoxP2BombCountWrite.MaxLength = 3;
            textBoxP2AmmoCountWrite.MaxLength = 5;
            textBoxP2VehicleAmmoCountWrite.MaxLength = 3;
            textBoxP2VehicleHealthCountWrite.MaxLength = 3;
            textBoxP2CreditsCountWrite.MaxLength = 3;
        }

        #endregion

        // Gets game connection or returns null if not found
        private Process GameConnect()
        {
            string processName = PROCESS_NAME;
            if (checkBoxProcessName.Checked)
            {
                processName = textBoxProcessName.Text;
            }

            // Game process: mslugx
            game = Process.GetProcessesByName(processName).FirstOrDefault();
            if (game == null)
            {
                hProc = IntPtr.Zero;
            }
            else
            {
                // Open handle with full permission to game process
                hProc = OpenProcess((int)ProcessAccessRights.PROCESS_ALL_ACCESS, false, game.Id);
            }
                
            return game;
        }

        private IntPtr GetBaseAddress()
        {
            string moduleName = MODULE_NAME;
            if (checkBoxModuleName.Checked)
            {
                moduleName = textBoxModuleName.Text;
            }

            IntPtr baseAddress = IntPtr.Zero;
            foreach (ProcessModule module in game.Modules)
            {
                if (module.ModuleName.ToLower() == moduleName)
                {
                    baseAddress = module.BaseAddress;
                    break;
                }
            }

            return baseAddress;
        }

        private IntPtr GetOffsetAddress(IntPtr hProc, int address, int offset)
        {
            IntPtr baseAddress = GetBaseAddress();
            IntPtr offsetAddress = IntPtr.Zero;
            if (baseAddress != IntPtr.Zero)
            {
                offsetAddress = IntPtr.Add(baseAddress, address);
                offsetAddress = ReadPointerUInt32(hProc, offsetAddress, offset);
            }
            return offsetAddress;
        }

        #region Display Value Methods

        private string DisplayByteValue(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            int tempValue = ReadByte(hProc, addressGlobal);
            return tempValue.ToString();
        }

        private string DisplayUInt16Value(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            uint tempValue = ReadUInt16(hProc, addressGlobal);
            return tempValue.ToString();
        }

        private string DisplayUInt32Value(IntPtr hProc, IntPtr addressGlobal, int baseAddress, int valueOffset)
        {
            if (addressGlobal == IntPtr.Zero || ((int)addressGlobal) == valueOffset)
            {
                addressGlobal = GetOffsetAddress(hProc, baseAddress, valueOffset);
            }
            uint tempValue = ReadUInt32(hProc, addressGlobal);
            return tempValue.ToString();
        }

        #endregion

        #region Get Pointer Methods

        private IntPtr ReadPointerInt16(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt16(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt16(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt16(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerInt32(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt32(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt32(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt32(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerInt64(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToInt64(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        private IntPtr ReadPointerUInt64(IntPtr hProcess, IntPtr address, int offset)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            IntPtr ptr = (IntPtr)BitConverter.ToUInt64(buffer, 0);
            return IntPtr.Add(ptr, offset);
        }

        #endregion

        #region Get/Set Memory Value Methods

        private float ReadFloat(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4]; // FLOAT = 4 byte
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        private void WriteFloat(IntPtr hProcess, IntPtr address, float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private int ReadInt(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4]; // INT = 4 byte
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        private void WriteInt(IntPtr hProcess, IntPtr address, int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private int ReadByte(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[1];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return buffer[0];
        }

        private void WriteByte(IntPtr hProcess, IntPtr address, byte value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private uint ReadUInt16(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToUInt16(buffer, 0);
        }

        private void WriteUInt16(IntPtr hProcess, IntPtr address, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        private uint ReadUInt32(IntPtr hProcess, IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesRead);
            return BitConverter.ToUInt32(buffer, 0);
        }

        private void WriteUInt32(IntPtr hProcess, IntPtr address, uint value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            WriteProcessMemory(hProcess, address, buffer, buffer.Length, out int bytesWritten);
        }

        #endregion

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((GameConnect() == null) || (GetBaseAddress() == IntPtr.Zero))
            {
                ForeColor = Color.Red;
                string logEntry = "Metal Slug X Process Not Found.";
                logEntry += Environment.NewLine;
                if (logEntry != previousLogEntry)
                {
                    textBoxLog.AppendText(logEntry + Environment.NewLine);
                    previousLogEntry = logEntry;
                }
            }
            else
            {
                ForeColor = Color.Green;
                string logEntry = $"[Metal Slug X Process {game.ProcessName} found in {game} with PID: {game.Id}]";
                logEntry += Environment.NewLine;
                logEntry += $"Start Time: {game.StartTime}";
                logEntry += Environment.NewLine;
                //logEntry += $"Total Processor Time: {game.TotalProcessorTime}";
                logEntry += $"Physical Memory Usage (MB): {game.WorkingSet64 / (1024 * 1024)}";
                logEntry += Environment.NewLine;
                logEntry += "---------------------------------------------------";
                logEntry += Environment.NewLine;

                if (logEntry != previousLogEntry)
                {
                    textBoxLog.AppendText(logEntry + Environment.NewLine);
                    previousLogEntry = logEntry;
                }

                levelTimerAddressGlobal              = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_LEVEL_TIMER                );
                continueTimerAddressGlobal           = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_CONTINUE_TIMER             );

                livesCountP1AddressGlobal            = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_LIVES_COUNT             );
                livesCountP1CompleteAddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_LIVES_COUNT_COMPLETE    );
                invincibilityTimerP1AddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_INVINCIBILITY_TIMER     );
                powsRescuedP1AddressGlobal           = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_POWS_RESCUED            );
                weaponTypeP1AddressGlobal            = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_WEAPON_TYPE             );
                bombTypeP1AddressGlobal              = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_BOMB_TYPE               );
                bombCountP1AddressGlobal             = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_BOMB_COUNT              );
                ammoCountP1AddressGlobal             = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_AMMO_COUNT              );
                vehicleAmmoCanonCountP1AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_AMMO_CANON_COUNT);
                vehicleHealthCountP1AddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_HEALTH_COUNT    );
                scoreP1AddressGlobal                 = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_SCORE                   );
                currentCharacterP1AddressGlobal      = GetOffsetAddress(hProc, BASE_ADDRESS_1, OFFSET_P1_CURRENT_CHARACTER       );
                creditsCountP1AddressGlobal          = GetOffsetAddress(hProc, BASE_ADDRESS_3, OFFSET_P1_CREDITS_COUNT           );

                livesCountP2AddressGlobal            = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_LIVES_COUNT             );
                livesCountP2CompleteAddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_LIVES_COUNT_COMPLETE    );
                invincibilityTimerP2AddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_INVINCIBILITY_TIMER     );
                powsRescuedP2AddressGlobal           = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_POWS_RESCUED            );
                weaponTypeP2AddressGlobal            = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_WEAPON_TYPE             );
                bombTypeP2AddressGlobal              = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_BOMB_TYPE               );
                bombCountP2AddressGlobal             = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_BOMB_COUNT              );
                ammoCountP2AddressGlobal             = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_AMMO_COUNT              );
                vehicleAmmoCanonCountP2AddressGlobal = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_AMMO_CANON_COUNT);
                vehicleHealthCountP2AddressGlobal    = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_HEALTH_COUNT    );
                scoreP2AddressGlobal                 = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_SCORE                   );
                currentCharacterP2AddressGlobal      = GetOffsetAddress(hProc, BASE_ADDRESS_2, OFFSET_P2_CURRENT_CHARACTER       );
                creditsCountP2AddressGlobal          = GetOffsetAddress(hProc, BASE_ADDRESS_3, OFFSET_P2_CREDITS_COUNT           );

                #region Level Timer

                if (checkBoxLevelTimer.Checked)
                {
                    try
                    {
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, continueTimerAddressGlobal, BASE_ADDRESS_1, OFFSET_CONTINUE_TIMER), out int continueTimer))
                        {
                            tempByte = Convert.ToByte(continueTimer);
                        }
                        if (int.TryParse(textBoxLevelTimerWrite.Text, out int levelTimer)
                            && (levelTimer >= 0 && levelTimer <= 255))
                        {
                            WriteByte(hProc, levelTimerAddressGlobal, Convert.ToByte(levelTimer));
                            // Write existing Continue Timer Count as it gets overwritten
                            WriteByte(hProc, continueTimerAddressGlobal, tempByte);
                        }
                        else
                        {
                            textBoxLog.AppendText("Level Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Level Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxLevelTimerRead.Text = DisplayByteValue(hProc, levelTimerAddressGlobal, BASE_ADDRESS_1, OFFSET_LEVEL_TIMER);

                #endregion

                #region Continue Timer

                if (checkBoxContinueTimer.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxContinueTimerWrite.Text, out int continueTimer)
                            && (continueTimer >= 0 && continueTimer <= 255))
                        {
                            WriteByte(hProc, continueTimerAddressGlobal, Convert.ToByte(continueTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("Continue Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Continue Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxContinueTimerRead.Text = DisplayByteValue(hProc, continueTimerAddressGlobal, BASE_ADDRESS_1, OFFSET_CONTINUE_TIMER);

                #endregion

                #region Lives Count

                if (checkBoxP1LivesCount.Checked)
                {
                    try
                    {
                        // 0 - 127 Normal Lives
                        // 128 - 255 - Infinite Lives
                        if (int.TryParse(textBoxP1LivesCountWrite.Text, out int livesCount)
                            && (livesCount >= 0 && livesCount <= 255))
                        {
                            // Write P1 number of lives
                            WriteByte(hProc, livesCountP1AddressGlobal, Convert.ToByte(livesCount));
                            // Write 0
                            WriteByte(hProc, livesCountP1CompleteAddressGlobal, 0);
                        }
                        else
                        {
                            textBoxLog.AppendText("Lives Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Lives Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1LivesCountRead.Text = DisplayByteValue(hProc, livesCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_LIVES_COUNT);

                if (checkBoxP2LivesCount.Checked)
                {
                    try
                    {
                        // 0 - 127 Normal Lives
                        // 128 - 255 - Infinite Lives
                        if (int.TryParse(textBoxP2LivesCountWrite.Text, out int livesCount)
                            && (livesCount >= 0 && livesCount <= 255))
                        {
                            // Write P2 number of lives
                            WriteByte(hProc, livesCountP2AddressGlobal, Convert.ToByte(livesCount));
                            // Write 0
                            WriteByte(hProc, livesCountP2CompleteAddressGlobal, 0);
                        }
                        else
                        {
                            textBoxLog.AppendText("Lives Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Lives Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2LivesCountRead.Text = DisplayByteValue(hProc, livesCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_LIVES_COUNT);

                #endregion

                #region Invincibility Timer

                if (checkBoxP1InvincibilityTimer.Checked)
                {
                    try
                    {
                        // Freeze at 60 - Invincible
                        if (int.TryParse(textBoxP1InvincibilityTimerWrite.Text, out int invincibilityTimer)
                            && (invincibilityTimer >= 0 && invincibilityTimer <= 255))
                        {
                            WriteByte(hProc, invincibilityTimerP1AddressGlobal, Convert.ToByte(invincibilityTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("Invincibility Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Invincibility Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1InvincibilityTimerRead.Text = DisplayByteValue(hProc, invincibilityTimerP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_INVINCIBILITY_TIMER);

                if (checkBoxP2InvincibilityTimer.Checked)
                {
                    try
                    {
                        // Freeze at 60 - Invincible
                        if (int.TryParse(textBoxP2InvincibilityTimerWrite.Text, out int invincibilityTimer)
                            && (invincibilityTimer >= 0 && invincibilityTimer <= 255))
                        {
                            WriteByte(hProc, invincibilityTimerP2AddressGlobal, Convert.ToByte(invincibilityTimer));
                        }
                        else
                        {
                            textBoxLog.AppendText("Invincibility Timer value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Invincibility Timer value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2InvincibilityTimerRead.Text = DisplayByteValue(hProc, invincibilityTimerP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_INVINCIBILITY_TIMER);

                #endregion

                #region POW Count

                if (checkBoxP1POWsRescued.Checked)
                {
                    try
                    {
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, powsRescuedP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_POWS_RESCUED), out int currentPowsRescuedP2))
                        {
                            tempByte = Convert.ToByte(currentPowsRescuedP2);
                        }
                        if (int.TryParse(textBoxP1POWsRescuedWrite.Text, out int powsRescued)
                            && (powsRescued >= 0 && powsRescued <= 255))
                        {
                            WriteByte(hProc, powsRescuedP1AddressGlobal, Convert.ToByte(powsRescued));
                            // Write existing P2 POW Rescued Count as P1 POW Rescued Count is the High Byte and overwrites the P2 POW Rescued Count (Low Byte)
                            WriteByte(hProc, powsRescuedP2AddressGlobal, tempByte);
                        }
                        else
                        {
                            textBoxLog.AppendText("POW's Rescued value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("POW's Rescued value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1POWsRescuedRead.Text = DisplayByteValue(hProc, powsRescuedP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_POWS_RESCUED);

                if (checkBoxP2POWsRescued.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2POWsRescuedWrite.Text, out int powsRescued)
                            && (powsRescued >= 0 && powsRescued <= 255))
                        {
                            WriteByte(hProc, powsRescuedP2AddressGlobal, Convert.ToByte(powsRescued));
                        }
                        else
                        {
                            textBoxLog.AppendText("POW's Rescued value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("POW's Rescued value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2POWsRescuedRead.Text = DisplayByteValue(hProc, powsRescuedP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_POWS_RESCUED);

                #endregion

                #region Bomb Count

                if (checkBoxP1BombCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1BombCountWrite.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP1AddressGlobal, Convert.ToByte(bombCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("Bomb Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Bomb value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1BombCountRead.Text = DisplayByteValue(hProc, bombCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_COUNT);

                if (checkBoxP2BombCount.Checked)
                {
                    try
                    {
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, bombCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_COUNT), out int currentBombCountP1))
                        {
                            tempByte = Convert.ToByte(currentBombCountP1);
                        }
                        if (int.TryParse(textBoxP2BombCountWrite.Text, out int bombCount)
                            && (bombCount >= 0 && bombCount <= 255))
                        {
                            WriteByte(hProc, bombCountP2AddressGlobal, Convert.ToByte(bombCount));
                            // Write existing P1 Bomb Count as P2 Bomb Count is the High Byte and overwrites the P1 Bomb Count (Low Byte)
                            WriteByte(hProc, bombCountP1AddressGlobal, tempByte);
                        }
                        else
                        {
                            textBoxLog.AppendText("Bomb Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Bomb Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2BombCountRead.Text = DisplayByteValue(hProc, bombCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_BOMB_COUNT);

                #endregion

                #region Vehicle Ammo Count

                if (checkBoxP1VehicleAmmoCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1VehicleAmmoCountWrite.Text, out int vehicleAmmoCount)
                            && (vehicleAmmoCount >= 0 && vehicleAmmoCount <= 255))
                        {
                            WriteByte(hProc, vehicleAmmoCanonCountP1AddressGlobal, Convert.ToByte(vehicleAmmoCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("Vehicle Ammo Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Vehicle Ammo Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1VehicleAmmoCountRead.Text = DisplayByteValue(hProc, vehicleAmmoCanonCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_AMMO_CANON_COUNT);

                if (checkBoxP2VehicleAmmoCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2VehicleAmmoCountWrite.Text, out int vehicleAmmoCount)
                            && (vehicleAmmoCount >= 0 && vehicleAmmoCount <= 255))
                        {
                            WriteByte(hProc, vehicleAmmoCanonCountP2AddressGlobal, Convert.ToByte(vehicleAmmoCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("Vehicle Ammo Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Vehicle Ammo Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2VehicleAmmoCountRead.Text = DisplayByteValue(hProc, vehicleAmmoCanonCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_AMMO_CANON_COUNT);

                #endregion

                #region Vehicle Health Count

                if (checkBoxP1VehicleHealthCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP1VehicleHealthCountWrite.Text, out int vehicleHealthCount)
                            && (vehicleHealthCount >= 0 && vehicleHealthCount <= 255))
                        {
                            WriteByte(hProc, vehicleHealthCountP1AddressGlobal, Convert.ToByte(vehicleHealthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("Vehicle Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Vehicle Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1VehicleHealthCountRead.Text = DisplayByteValue(hProc, vehicleHealthCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_VEHICLE_HEALTH_COUNT);

                if (checkBoxP2VehicleHealthCount.Checked)
                {
                    try
                    {
                        if (int.TryParse(textBoxP2VehicleHealthCountWrite.Text, out int vehicleHealthCount)
                            && (vehicleHealthCount >= 0 && vehicleHealthCount <= 255))
                        {
                            WriteByte(hProc, vehicleHealthCountP2AddressGlobal, Convert.ToByte(vehicleHealthCount));
                        }
                        else
                        {
                            textBoxLog.AppendText("Vehicle Health Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Vehicle Health Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2VehicleHealthCountRead.Text = DisplayByteValue(hProc, vehicleHealthCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_VEHICLE_HEALTH_COUNT);

                #endregion

                #region Credits Count

                if (checkBoxP1CreditsCount.Checked)
                {
                    try
                    {
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, creditsCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_CREDITS_COUNT), out int currentCreditsCountP2))
                        {
                            tempByte = Convert.ToByte(currentCreditsCountP2);
                        }
                        if (int.TryParse(textBoxP1CreditsCountWrite.Text, out int creditsCount)
                            && (creditsCount >= 0 && creditsCount <= 255))
                        {
                            WriteByte(hProc, creditsCountP1AddressGlobal, Convert.ToByte(creditsCount));
                            WriteByte(hProc, creditsCountP2AddressGlobal, tempByte);
                        }
                        else
                        {
                            textBoxLog.AppendText("Credits Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Credits Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1CreditsCountRead.Text = DisplayByteValue(hProc, creditsCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_CREDITS_COUNT);

                if (checkBoxP2CreditsCount.Checked)
                {
                    try
                    {
                        //byte tempByte = 0;
                        //if (int.TryParse(DisplayByteValue(hProc, creditsCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_CREDITS_COUNT), out int currentCreditsCountP1))
                        //{
                        //    tempByte = Convert.ToByte(currentCreditsCountP1);
                        //}
                        if (int.TryParse(textBoxP2CreditsCountWrite.Text, out int creditsCount)
                            && (creditsCount >= 0 && creditsCount <= 255))
                        {
                            WriteByte(hProc, creditsCountP2AddressGlobal, Convert.ToByte(creditsCount));
                            // Write existing P1 Credits Count as P2 Credits Count is the High Byte and overwrites the P1 Credits Count (Low Byte)
                            //WriteByte(hProc, creditsCountP1AddressGlobal, tempByte);
                        }
                        else
                        {
                            textBoxLog.AppendText("Credits Count value must be between 0 and 255." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Credits Count value must be between 0 and 255."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2CreditsCountRead.Text = DisplayByteValue(hProc, creditsCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_CREDITS_COUNT);

                #endregion

                #region Ammo Count

                // Ammo is 2 bytes
                if (checkBoxP1AmmoCount.Checked)
                {
                    try
                    {
                        uint tempInt = 0;
                        if (uint.TryParse(DisplayByteValue(hProc, ammoCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_AMMO_COUNT), out uint currentAmmoCountP2))
                        {
                            tempInt = Convert.ToByte(currentAmmoCountP2);
                        }
                        if (uint.TryParse(textBoxP1AmmoCountWrite.Text, out uint ammoCount)
                            && (ammoCount >= 0 && ammoCount <= 65535))
                        {
                            WriteUInt16(hProc, ammoCountP1AddressGlobal, ammoCount);
                            // Write existing P2 Ammo Count as P1 Ammo Count is the High Byte and overwrites the P2 Ammo Count (Low Byte)
                            WriteUInt16(hProc, ammoCountP2AddressGlobal, tempInt);
                        }
                        else
                        {
                            textBoxLog.AppendText("Ammo Count value must be between 0 and 65535." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Ammo value must be between 0 and 65535."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP1AmmoCountRead.Text = DisplayUInt16Value(hProc, ammoCountP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_AMMO_COUNT);

                if (checkBoxP2AmmoCount.Checked)
                {
                    try
                    {
                        
                        if (uint.TryParse(textBoxP2AmmoCountWrite.Text, out uint ammoCount)
                            && (ammoCount >= 0 && ammoCount <= 65535))
                        {
                            WriteUInt16(hProc, ammoCountP2AddressGlobal, ammoCount);
                        }
                        else
                        {
                            textBoxLog.AppendText("Ammo Count value must be between 0 and 65535." + Environment.NewLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("Ammo value must be between 0 and 65535."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                textBoxP2AmmoCountRead.Text = DisplayUInt16Value(hProc, ammoCountP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_AMMO_COUNT);

                #endregion

                #region Score

                // Score is 4 bytes
                if (checkBoxP1Score.Checked)
                {
                    // 12345678
                    // | C46B | C46A | C46D | C46C |
                    // | 12   | 34   | 56   | 78   |
                    int c46B = comboBoxP1Score1.SelectedIndex;
                    int c46A = comboBoxP1Score2.SelectedIndex;
                    int c46D = comboBoxP1Score3.SelectedIndex;
                    int c46C = comboBoxP1Score4.SelectedIndex;

                    WriteByte(hProc, scoreP1AddressGlobal, (byte)c46A);
                    WriteByte(hProc, scoreP1AddressGlobal + 1, (byte)c46B);
                    WriteByte(hProc, scoreP1AddressGlobal + 2, (byte)c46C);
                    WriteByte(hProc, scoreP1AddressGlobal + 3, (byte)c46D);
                }

                int tempValue2 = ReadByte(hProc, scoreP1AddressGlobal + 1);
                textBoxP1ScoreRead.Text = tempValue2.ToString("X").PadLeft(2, '0');
                int tempValue = ReadByte(hProc, scoreP1AddressGlobal);
                textBoxP1ScoreRead.Text += tempValue.ToString("X").PadLeft(2, '0');
                int tempValue4 = ReadByte(hProc, scoreP1AddressGlobal + 3);
                textBoxP1ScoreRead.Text += tempValue4.ToString("X").PadLeft(2, '0');
                int tempValue3 = ReadByte(hProc, scoreP1AddressGlobal + 2);
                textBoxP1ScoreRead.Text += tempValue3.ToString("X").PadLeft(2, '0');


                if (checkBoxP2Score.Checked)
                {

                    int c473 = comboBoxP2Score1.SelectedIndex;
                    int c472 = comboBoxP2Score2.SelectedIndex;
                    int c475 = comboBoxP2Score3.SelectedIndex;
                    int c474 = comboBoxP2Score4.SelectedIndex;

                    WriteByte(hProc, scoreP2AddressGlobal, (byte)c472);
                    WriteByte(hProc, scoreP2AddressGlobal + 1, (byte)c473);
                    WriteByte(hProc, scoreP2AddressGlobal + 2, (byte)c474);
                    WriteByte(hProc, scoreP2AddressGlobal + 3, (byte)c475);
                }

                tempValue2 = ReadByte(hProc, scoreP2AddressGlobal + 1);
                textBoxP2ScoreRead.Text = tempValue2.ToString("X").PadLeft(2, '0');
                tempValue = ReadByte(hProc, scoreP2AddressGlobal);
                textBoxP2ScoreRead.Text += tempValue.ToString("X").PadLeft(2, '0');
                tempValue4 = ReadByte(hProc, scoreP2AddressGlobal + 3);
                textBoxP2ScoreRead.Text += tempValue4.ToString("X").PadLeft(2, '0');
                tempValue3 = ReadByte(hProc, scoreP2AddressGlobal + 2);
                textBoxP2ScoreRead.Text += tempValue3.ToString("X").PadLeft(2, '0');

                #endregion

                #region Weapon Type

                if (checkBoxP1WeaponType.Checked)
                {
                    
                    try
                    {
                        //string value = ((KeyValuePair<int, string>)comboBoxP1WeaponTypeWrite.SelectedItem).Value;
                        WriteByte(hProc, weaponTypeP1AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP1WeaponTypeWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 1 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, weaponTypeP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_WEAPON_TYPE), out int weaponTypeP1))
                {
                    try
                    {
                        comboBoxP1WeaponTypeRead.SelectedIndex = weaponTypeP1;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 1 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                if (checkBoxP2WeaponType.Checked)
                {
                    try
                    {
                        // 2 bytes hold P1 and P2 weapon types, changing P2 sets P1 to 0, they probably share the same 16-bit value. P1 is the upper byte, P2 is the lower byte
                        // Get the current P1 weapon type.
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, weaponTypeP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_WEAPON_TYPE), out int currentWeaponTypeP1))
                        {
                            tempByte = Convert.ToByte(currentWeaponTypeP1);
                        }
                        
                        // Write new P2 weapon type
                        WriteByte(hProc, weaponTypeP2AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP2WeaponTypeWrite.SelectedItem).Key));
                        // Write existing P1 Weapon Type as P2 Weapon Type is the High Byte and overwrites the P1 Weapon Type (Low Byte)
                        WriteByte(hProc, weaponTypeP1AddressGlobal, tempByte);
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 2 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, weaponTypeP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_WEAPON_TYPE), out int weaponTypeP2))
                {
                    try
                    {
                        comboBoxP2WeaponTypeRead.SelectedIndex = weaponTypeP2;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 2 Weapon Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                #endregion

                #region Bomb Type

                if (checkBoxP1BombType.Checked)
                {
                    try
                    {
                        WriteByte(hProc, bombTypeP1AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP1BombTypeWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 1 Bomb Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, bombTypeP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_TYPE), out int bombTypeP1))
                {
                    try
                    {
                        comboBoxP1BombTypeRead.SelectedIndex = bombTypeP1;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 1 Bomb Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                if (checkBoxP2BombType.Checked)
                {
                    try
                    {
                        byte tempByte = 0;
                        if (int.TryParse(DisplayByteValue(hProc, bombTypeP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_BOMB_TYPE), out int currentBombTypeP1))
                        {
                            tempByte = Convert.ToByte(currentBombTypeP1);
                        }
                        WriteByte(hProc, bombTypeP2AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP2BombTypeWrite.SelectedItem).Key));
                        // Write existing P1 Bomb Type as P2 Bomb Type is the High Byte and overwrites the P1 Bomb Type (Low Byte)
                        WriteByte(hProc, bombTypeP1AddressGlobal, tempByte);
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 2 Bomb Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, bombTypeP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_BOMB_TYPE), out int bombTypeP2))
                {
                    try
                    {
                        comboBoxP2BombTypeRead.SelectedIndex = bombTypeP2;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 2 Bomb Type."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                #endregion

                #region Current Character

                if (checkBoxP1CurrentCharacter.Checked)
                {
                    try
                    {
                        WriteByte(hProc, currentCharacterP1AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP1CurrentCharacterWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 1 Current Character."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, currentCharacterP1AddressGlobal, BASE_ADDRESS_1, OFFSET_P1_CURRENT_CHARACTER), out int currentCharacterP1))
                {
                    try
                    {
                        comboBoxP1CurrentCharacterRead.SelectedIndex = currentCharacterP1;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 1 Current Character."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                if (checkBoxP2CurrentCharacter.Checked)
                {
                    try
                    {
                        WriteByte(hProc, currentCharacterP2AddressGlobal, Convert.ToByte(((KeyValuePair<int, string>)comboBoxP2CurrentCharacterWrite.SelectedItem).Key));
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred setting Player 2 Current Character."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }
                if (int.TryParse(DisplayByteValue(hProc, currentCharacterP2AddressGlobal, BASE_ADDRESS_2, OFFSET_P2_CURRENT_CHARACTER), out int currentCharacterP2))
                {
                    try
                    {
                        comboBoxP2CurrentCharacterRead.SelectedIndex = currentCharacterP2;
                    }
                    catch (Exception ex)
                    {
                        textBoxLog.AppendText("An error occurred getting Player 2 Current Character."
                            + Environment.NewLine
                            + "Exception: "
                            + ex);
                    }
                }

                #endregion

            }
        }

        #endregion

    }
}
