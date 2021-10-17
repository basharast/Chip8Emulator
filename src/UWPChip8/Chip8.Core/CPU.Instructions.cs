using System;

namespace Chip8.Core
{
    public partial class CPU
    {
        public void ExecuteOpCode()
        {
            try
            {
                switch (_opcode.InstructionType)
                {
                    case 0x0000:
                        switch (_opcode.Value)
                        {
                            case 0x00E0: // CLS
                                CLS();
                                break;
                            case 0x00EE: // RET
                                RET();
                                break;
                        }
                        break;
                    case 0x1000: //JP addr
                        JP(_opcode.NNN);
                        break;
                    case 0x2000: // CALL addr
                        Call(_opcode.NNN);
                        break;
                    case 0x3000: // SE Vx, byte
                        SE(_opcode.X, (byte)(_opcode.NN));
                        break;
                    case 0x4000: // SNE Vx, byte
                        SNE(_opcode.X, _opcode.NN);
                        break;
                    case 0x5000: // SE Vx, Vy
                        SE(_opcode.X, _v[_opcode.Y]);
                        break;
                    case 0x6000: // LD Vx, byte
                        LD(_opcode.X, _opcode.NN);
                        break;
                    case 0x7000: // ADD Vx, byte
                        ADD(_opcode.X, _opcode.NN);
                        break;
                    case 0x8000:
                        switch (_opcode.N)
                        {
                            case 0x0000: // LD Vx, Vy
                                LD(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0001: // OR Vx, Vy
                                OR(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0002: // AND Vx, Vy
                                AND(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0003: // XOR Vx, Vy
                                XOR(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0004: // ADD Vx, Vy
                                ADD(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0005: // SUB Vx, Vy
                                SUB(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x0006: // SHR Vx, 1
                                SHR(_opcode.X);
                                break;
                            case 0x0007: // SUBN Vx, Vy
                                SUBN(_opcode.X, _v[_opcode.Y]);
                                break;
                            case 0x000E: // SHL Vx, 1
                                SHL(_opcode.X);
                                break;
                        }
                        break;
                    case 0x9000: // SNE Vx, Vy
                        SNE(_opcode.X, _v[_opcode.Y]);
                        break;
                    case 0xA000: // LD I, addr
                        LD_I(_opcode.NNN);
                        break;
                    case 0xB000: // JP V0, addr
                        JP((ushort)(_opcode.NNN + _v[0]));
                        break;
                    case 0xC000: // RND Vx, byte
                        RND(_opcode.X, _opcode.NN);
                        break;
                    case 0xD000: // DRW Vx, Vy, nibble
                        DRW(_opcode.X, _opcode.Y, _opcode.N);
                        break;
                    case 0xE000: // Input handling
                        switch (_opcode.NN)
                        {
                            case 0x009E: // SKP Vx
                                SKP(_v[_opcode.X]);
                                break;
                            case 0x00A1: // SKNP Vx
                                SKNP(_v[_opcode.X]);
                                break;
                        }
                        break;
                    case 0xF000:
                        switch (_opcode.NN)
                        {
                            case 0x0007: // LD Vx, DT
                                LD(_opcode.X, (byte)DelayTimer.Get());
                                break;
                            case 0x000A: // LD Vx, K. TODO : Rough handle for checking key input
                                _halted = true;

                                for (var i = 0; i < 16; ++i)
                                {
                                    if (keys[i] != 0)
                                    {
                                        _v[_opcode.X] = (byte)i;

                                        _halted = false;
                                        continue;
                                    }
                                }
                                break;
                            case 0x0015: // LD DT, Vx
                                DelayTimer.Set(_v[_opcode.X]);
                                break;
                            case 0x0018: // LD ST, Vx,
                                SoundTimer.Set(_v[_opcode.X]);
                                break;
                            case 0x001E: // ADD I, Vx
                                I.Set(I.Value + _v[_opcode.X]);
                                break;
                            case 0x0029: // LD F, Vx
                                I.Set(_v[_opcode.X] * 5);
                                break;
                            case 0x0033: // LD B, Vx
                                BCD(_opcode.X);
                                break;
                            case 0x0055: // LD [I], Vx
                                for (byte i = 0; i <= _opcode.X; i++)
                                {
                                    _memory[I.Value + i] = _v[i];
                                }
                                break;
                            case 0x0065: // LD Vx, [I]
                                for (byte i = 0; i <= _opcode.X; i++)
                                {
                                    LD(i, _memory[I.Value + i]);
                                }
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Clears the display buffer
        /// </summary>
        void CLS()
        {
            try
            {
                for (int i = 0; i < _displayBuffer.Length; i++)
                {
                    _displayBuffer[i] = false;
                }
                DrawFlag = true;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Return from a subroutine
        /// </summary>
        void RET()
        {
            try
            {
                _pc = _stack[--_sp];
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// ADD Vx, kk
        /// 
        /// Adds Vx to kk and stores result at Vx
        /// 
        /// Sets carry register if necessary
        /// 
        /// Used By : 0x8xy4
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void ADD(byte ax, byte kk)
        {
            try
            {
                // TODO : Check this. This currently works because the operator + on two 
                // bytes is implicitly an integer and therefore can exceed 255. Dirty.
                _v[0xF] = (byte)((_v[ax] + kk > 255) ? 1 : 0);
                _v[ax] += kk;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// JP kkk
        /// 
        /// Jumps to address kkk
        /// </summary>
        /// <param name="kkk">kkk</param>
        void JP(ushort kkk)
        {
            try
            {
                _pc = kkk;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Calls subroutine at the specified address and stores previous address on stack, advancing stack pointer.
        /// </summary>
        /// <param name="addr">addr</param>
        void Call(ushort addr)
        {
            try
            {
                _stack[_sp] = _pc;
                _pc = addr;
                _sp++;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SE Vx, kk : 0x5XY0, 0x3XKK
        /// 
        /// Skip next instruction if Vx is equal to kk
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SE(byte ax, byte kk)
        {
            try
            {
                if (_v[ax] == kk)
                {
                    _pc += 2;
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SUB Vx, kk
        /// 
        /// Subtracts kk from Vx and stores value at Vx
        /// 
        /// Sets Vf if borrow not performed
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SUB(byte ax, byte kk)
        {
            try
            {
                _v[0x0f] = (byte)((_v[ax] >= kk) ? 1 : 0);
                _v[ax] -= kk;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SHR Vx : 0x8XY6
        /// 
        /// Logical Shift Right (1)
        /// 
        /// Sets Vf to LSB
        /// </summary>
        /// <param name="vx">ax</param>
        void SHR(byte ax)
        {
            try
            {
                _v[0xf] = (byte)(_v[ax] & 0x1);
                _v[ax] /= 2;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SUBN Vx, kk
        /// 
        /// Subtracts Vx from kk and stores value in Vx
        /// 
        /// Sets Vf if borrow not performed.
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SUBN(byte ax, byte kk)
        {
            try
            {
                _v[0xf] = (byte)((_v[ax] <= kk) ? 1 : 0);
                _v[ax] = (byte)(kk - _v[ax]);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SHL Vx
        /// 
        /// Logical Shift Left (1)
        /// 
        /// Sets Vf to MSB
        /// </summary>
        /// <param name="ax">ax</param>
        void SHL(byte ax)
        {
            try
            {
                _v[0xf] = (byte)(_v[ax] >> 7);
                _v[ax] *= 2;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// SNE Vx, kk
        /// 
        /// Skips next instruction if Vx does not equal kk
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void SNE(byte ax, byte kk)
        {
            try
            {
                if (_v[ax] != kk)
                    _pc += 2;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// OR Vx, kk
        /// 
        /// Bitwise OR on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void OR(byte ax, byte kk)
        {
            try
            {
                _v[ax] = (byte)(_v[ax] | kk);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// AND Vx, kk
        /// 
        /// Bitwise AND on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void AND(byte ax, byte kk)
        {
            try
            {
                _v[ax] = (byte)(_v[ax] & kk);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// XOR Vx, kk
        /// 
        /// Bitwise exclusive OR on Vx and kk and store result in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void XOR(byte ax, byte kk)
        {
            try
            {
                _v[ax] = (byte)(0xFF & (_v[ax] ^ kk));
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// LD I, nnnn : ANNN
        /// 
        /// Load value into I register.
        /// </summary>
        /// <param name="value">Value.</param>
        void LD_I(int value)
        {
            try
            {
                I.Set(value);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// RND Vx, kk : CXKK
        /// 
        /// Generates a random byte bitwise AND with kk and result stored in Vx
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void RND(byte ax, byte kk)
        {
            try
            {
                byte rndVal = (byte)_rnd.Next(0, 255 + 1);
                _v[ax] = (byte)(rndVal & kk);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Loads value kk into address ax
        /// </summary>
        /// <param name="ax">ax</param>
        /// <param name="kk">kk</param>
        void LD(byte ax, byte kk)
        {
            try
            {
                _v[ax] = kk;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Writes a pixel to the display buffer and sets the collision flag
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="height"></param>
        void DRW(byte ax, byte ay, byte height)
        {
            try
            {
                byte pixel;

                _v[0xF] = 0;
                for (int y = 0; y < height; y++)
                {
                    pixel = _memory[I.Value + y];
                    for (int x = 0; x < 8; x++)
                    {
                        if ((pixel & (0x80 >> x)) != 0 && ((_v[ay] + y < Config.ScreenHeight)))
                        {
                            _displayBuffer[(_v[ax] + x + ((_v[ay] + y) * Config.ScreenWidth))] ^= true;
                            _v[0xF] = _displayBuffer[_v[ax] + x + ((_v[ay] + y) * Config.ScreenWidth)] ? (byte)0 : (byte)1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Skips next instruction is key IS pressed
        /// </summary>
        /// <param name="ax">ax</param>
        void SKP(byte ax)
        {
            try
            {
                if (keys[ax] == 1)
                    _pc += 2;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Skips next instruction if key is NOT pressed
        /// </summary>
        /// <param name="ax">ax</param>
        void SKNP(byte ax)
        {
            try
            {
                if (this.keys[ax] != 1)
                    this._pc += 2;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Stores BCD representation of Vx in memory locations I, I+1, I+2
        /// 
        /// This is a direct implementation of Alexander Dicksons BCD function
        /// </summary>
        /// <param name="ax">ax</param>
        void BCD(byte ax)
        {
            try
            {
                // This appears to be used primarily for scoreboards
                var number = _v[ax];

                for (int i = 3; i > 0; i--)
                {
                    _memory[I.Value + i - 1] = (byte)(number % 10);
                    number /= 10;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}