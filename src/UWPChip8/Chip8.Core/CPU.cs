using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Chip8.Core
{
    public partial class CPU
    {
        #region Fields

        byte[] _memory;
        int[] _stack = new int[16];
        int _sp;
        int _pc;
        byte[] _v;
        OpCode _opcode = new OpCode();
        Random _rnd = new Random();
        bool[] _displayBuffer;
        int[] keys = new int[16];
        bool _halted = false;
        bool _poweredUp = false;

        #endregion

        #region Constructors

        public CPU()
        {
        }

        #endregion

        #region Properties

        public bool[] DisplayBuffer { get { return _displayBuffer; } }

        public bool DrawFlag { get; set; } = false;

        public bool PoweredUp { get { return _poweredUp; } }

        bool isSoundActive;
        bool isAudioTimerInProgress;
        System.Threading.Timer AudioTimer;
        private void callConnectionTimer(bool startState = false)
        {
            try
            {
                AudioTimer?.Dispose();
                if (startState)
                {
                    AudioTimer = new System.Threading.Timer(async delegate { if (isAudioTimerInProgress) { return; } else { CheckForAudio(); } }, null, 0, 250);
                }
            }
            catch (Exception e)
            {
            }
        }
        private async void CheckForAudio()
        {
            isAudioTimerInProgress = true;
            if (SoundTimer.Get() == 0)
            {
               foreach(var eItem in EffectsTemp)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                    {
                        try
                        {
                            if (eItem.Value.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
                            {
                                eItem.Value.Stop();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        isAudioTimerInProgress = false;
                    });
                }
            }
        }
        public void Initialize(bool audioState)
        {
            isSoundActive = audioState;
            callConnectionTimer(true);
            try
            {
                // Resets registers
                _v = new byte[16];
                I.Set(0);

                // Allocates memory
                _memory = new byte[4 * 1024];

                // Resets the display buffer
                _displayBuffer = new bool[Config.ScreenWidth * Config.ScreenHeight];

                // Set our program counter to the start
                _pc = 0x200;

                // Resets the timers
                DelayTimer.Set(0);
                SoundTimer.Set(0);

                // Reset our key states
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = 0;

                // Power state is handled once the ROM is loaded. We can theoretically initialize the CPU without powering
                // up the rom so we can use this method to re-initialize if necessary.
                _poweredUp = false;
                _halted = true;
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Methods

        public void PowerUp(Cart cart)
        {
            try
            {
                // Copy the carts bytes to the starting mem of 0x200
                Array.Copy(cart.Bytes, 0, _memory, 0x200, cart.Bytes.Length);

                // Reproduced with thanks from Alex Dicksons JS Chip-8 Emulator
                var hexChars = new byte[]{
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
			  0x20, 0x60, 0x20, 0x20, 0x70, // 1
			  0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
			  0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
			  0x90, 0x90, 0xF0, 0x10, 0x10, // 4
			  0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
			  0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
			  0xF0, 0x10, 0x20, 0x40, 0x40, // 7
			  0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
			  0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
			  0xF0, 0x90, 0xF0, 0x90, 0x90, // A
			  0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
			  0xF0, 0x80, 0x80, 0x80, 0xF0, // C
			  0xE0, 0x90, 0x90, 0x90, 0xE0, // D
			  0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
			  0xF0, 0x80, 0xF0, 0x80, 0x80  // F
			};

                Array.Copy(hexChars, 0, _memory, 0x0, hexChars.Length);

                _poweredUp = true;
                _halted = false;
            }
            catch (Exception ex)
            {

            }
        }

        public void SetKey(int index, int val)
        {
            try
            {
                keys[index] = val;
            }
            catch (Exception ex)
            {

            }
        }

        public int GetKey(int index)
        {
            return keys[index];
        }
        public void IterateCycle()
        {
            try
            {
                if (!_halted)
                {
                    _opcode.Set((ushort)((_memory[_pc] << 8) | _memory[_pc + 1]));
                    _pc += 2;
                }

                ExecuteOpCode();
            }
            catch (Exception ex)
            {

            }
        }

        string audioFile = "long1.mp3";
        public void SetAudioTune(string tune)
        {
            audioFile = $"{tune}.mp3";
        }
        public void UpdateTimers()
        {
            try
            {
                if (DelayTimer.Get() > 0)
                    DelayTimer.Decrement();

                if (SoundTimer.Get() > 0)
                {
                    if (!isBeepOn)
                    {
                        beep(audioFile);
                    }
                    SoundTimer.Decrement();
                }
                else
                {
                    if (isBeepOn)
                    {
                        stop(audioFile);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        bool isBeepOn = false;
        public async void beep(string TargetSound)
        {
            if (!isSoundActive)
            {
                return;
            }
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
                {
                    try
                    {
                        double MediaVolume = 1;

                        if (CoreApplication.MainView != null && CoreApplication.MainView.CoreWindow != null)
                        {
                            try
                            {
                                await PlayMediaDirect(TargetSound, MediaVolume);
                            }
                            catch (Exception e)
                            {
                            }

                        }
                    }
                    catch (Exception e)
                    {
                    }
                });
            }
            catch (Exception ex)
            {

            }
            isBeepOn = true;
        }
        public Dictionary<string, MediaElement> EffectsTemp = new Dictionary<string, MediaElement>();
        public async Task PlayMediaDirect(string TargetSound, double MediaVolume)
        {
            MediaElement TempEffect;
            if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
            {
                TempEffect.Play();
            }
            else
            {
                MediaElement NotificationSound = new MediaElement();
                StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync($@"Assets\SFX");
                StorageFile file = await folder.GetFileAsync(TargetSound);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                NotificationSound.SetSource(stream, "audio/mp3");
                if (!EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                {
                    EffectsTemp.Add(TargetSound, NotificationSound);
                }
                NotificationSound.Play();
            }
        }
        public async void stop(string TargetSound)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                await Task.Delay(50);
                try
                {
                    MediaElement TempEffect;
                    if (EffectsTemp.TryGetValue(TargetSound, out TempEffect))
                    {
                        TempEffect.Stop();
                        TempEffect.Volume = 0;
                    }
                }
                catch (Exception e)
                {

                }
                isBeepOn = false;
            });
        }
    }
}
