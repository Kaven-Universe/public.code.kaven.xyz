﻿using System.Diagnostics;

namespace Study
{
    public class FloatingPoint
    {
        public class DoubleValue(double value, string? name = default)
        {
            public double Value = value;
            public string? Name = name;

            public override string ToString()
            {
                return Name?.ToString() ?? Value.ToString();
            }

            public static implicit operator double(DoubleValue v) => v.Value;
        }

        private static void Assert()
        {
            var v = Math.Pow(2, 24);

            var f = Convert.ToSingle(v);

            Debug.Assert(f == v);

            Debug.Assert(f - 1 == v - 1);
            Debug.Assert(f + 1 != v + 1);

            var i = 10;
            while (i-- > 0)
            {
                Debug.WriteLine(f++);
            }

            i = 10;
            while (i-- > 0)
            {
                Debug.WriteLine(f--);
            }
        }

        private static string GenerateMarkdownTable(byte[] byteArray)
        {
            // Header row
            var table = "| Byte |";
            for (var i = 0; i < byteArray.Length; i++)
            {
                table += $" Byte {i + 1} |";
            }
            table += "\n|-------|";
            for (var i = 0; i < byteArray.Length; i++)
            {
                table += "--------|";
            }

            // Hex row
            table += "\n| Hex   |";
            foreach (var byteValue in byteArray)
            {
                table += $" 0x{byteValue:X2} |";
            }

            // Decimal row
            table += "\n| Dec   |";
            foreach (var byteValue in byteArray)
            {
                table += $" {byteValue} |";
            }

            // Binary row
            table += "\n| Bin   |";
            foreach (var byteValue in byteArray)
            {
                table += $" {Convert.ToString(byteValue, 2).PadLeft(8, '0')} |";
            }

            return table;
        }

        private static string BuildRow(params string[] cells)
        {
            var row = "| " + string.Join(" | ", cells) + " |";
            return row;
        }

        private static string MoveDot(string value, int offset)
        {
            if (offset == 0)
            {
                return value;
            }

            string left;
            string right;

            var dotIndex = value.IndexOf('.');
            if (dotIndex != -1)
            {
                if (value.Count(p => p == '.') != 1)
                {
                    throw new Exception();
                }

                left = value[..dotIndex];
                right = value[(dotIndex + 1)..];
            }
            else
            {
                left = value;
                right = string.Empty;
            }

            var abs = Math.Abs(offset);
            if (offset < 0)
            {
                while (left.Length - 1 < abs)
                {
                    left = "0" + left;
                }

                left = left.Insert(left.Length - abs, ".");
            }
            else
            {
                while (right.Length < abs)
                {
                    right += "0";
                }

                right = right.Insert(abs, ".");
            }

            var result = left + right;

            //if (result.Contains('.'))
            //{
            //    result = result.TrimEnd('0').TrimEnd('.');
            //}

            return result;
        }



        private static string GenerateTable(DoubleValue[] values, bool sem = true)
        {
            var columns = new List<string>
            {
                "Number"
            };

            if (sem)
            {
                columns.Add("Sign (s, 1 bit)");
                columns.Add("Stored exponent (e, 11 bits)");
                columns.Add("Mantissa (m, 52 bits)");
            }
            else
            {
                columns.Add("$(-1)^s * 1.m_{(2)} * 2^{e-1023}$");
                //columns.Add(string.Empty);

                columns.Add("Fraction");
                columns.Add("Exact decimal representation");
                columns.Add("Value (.NET)");
            }

            var header = BuildRow([.. columns]);
            var rows = new List<string>()
            {
                header,
                string.Join(" -- ", Enumerable.Repeat('|', header.Count(p => p == '|'))),
            };

            (string s, string e, string m) extract1(double value)
            {
                var bin = Functions.BytesToBinaryString(BitConverter.GetBytes(value).Reverse().ToArray());
                var s = bin[..1];
                var e = bin.Substring(1, 11);
                var m = bin[12..];

                return (s, e, m);
            }

            (string s, string e, string m) extract2(double value)
            {
                // Get the binary representation of the double
                var bits = BitConverter.DoubleToInt64Bits(value);

                // Extract the sign, exponent, and mantissa
                var sign = (int)((bits >> 63) & 1);
                var exponent = (int)((bits >> 52) & 0x7FF);
                var mantissa = bits & 0xFFFFFFFFFFFFF;

                var s = sign > 0 ? "1" : "0";
                var e = exponent.ToString("b").PadLeft(11, '0');
                var m = mantissa.ToString("b").PadLeft(52, '0');

                return (s, e, m);
            }

            (string s, string e, string m) extract3(double value)
            {
                // Get the bytes of the double
                var bytes = BitConverter.GetBytes(value);

                // Extract the sign, exponent, and mantissa
                var sign = (bytes[7] >> 7) & 1;
                var exponent = ((bytes[7] & 0x7F) << 4) | ((bytes[6] >> 4) & 0xF);
                var mantissa = BitConverter.ToUInt64(bytes, 0) & 0x000FFFFFFFFFFFFF;

                var s = sign > 0 ? "1" : "0";
                var e = exponent.ToString("b").PadLeft(11, '0');
                var m = mantissa.ToString("b").PadLeft(52, '0');

                return (s, e, m);
            }

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];

                var v1 = extract1(value);
                var v2 = extract2(value);
                var v3 = extract3(value);

                if (v1 != v2 || v2 != v3)
                {
                    throw new Exception();
                }

                var (s, e, m) = v1;

                Trace.Assert(e.Length == 11);
                Trace.Assert(m.Length == 52);

                var cells = new List<string>
                {
                    value.ToString()
                };

                if (sem)
                {
                    cells.Add(s);
                    cells.Add(e);
                    cells.Add(m.AddSpaces());
                }
                else
                {
                    cells.Add($"$(-1)^{s} * 1.{m}_{{(2)}} * 2^{{{Convert.ToUInt64(e, 2)} - 1023}}$");

                    var sign = s == "0" ? 1 : -1; //Math.Pow(-1, s == "0" ? 0 : 1);

                    var offset = Convert.ToInt32(e, 2) - 1023;
                    var binStr = MoveDot($"1.{m}", offset);
                    //cells.Add($"${sign} * {binStr}_{{(2)}}$");

                    var f = Functions.BinaryStringToFraction(binStr);
                    var dec = (sign * f).ToString(int.MaxValue);

                    Trace.Assert(Functions.ToExactString(value) == dec);

                    cells.Add($"${sign} * \\frac{{{f.Numerator}}}{{{f.Denominator}}}$");
                    cells.Add(dec);
                    cells.Add($"{sign * Functions.BinaryStringToDouble(binStr)}");
                }

                var row = "| " + string.Join(" | ", cells) + " |";

                rows.Add(row);
            }

            return string.Join(Environment.NewLine, rows);
        }

        public static void Run()
        {
            //Functions.ToExactString(0.333d);

            //Assert();

            //var d1 = 46.42829231507700882275457843206822872161865234375;
            //var d2 = Math.Pow(2, 24);

            //var s = Functions.ToExactString(d);
            //var t1 = GenerateMarkdownTable(BitConverter.GetBytes(d));

            DoubleValue[] values = [
                new(1 / 3d, @"$\frac{1}{3}$"),
                new(1 / 2d, @"$\frac{1}{2}$"),
                new(1 / 10d, @"$\frac{1}{10}$"),
                new(Math.Sqrt(2), @"$\sqrt2$"),
                new(Math.Sqrt(3), @"$\sqrt3$"),
                new(Math.PI, "π"),
                new(Math.E, "e"),
                new(46.42829231507700882275457843206822872161865234375d, "46.42829231507700882275457843206822872161865234375"),
                new(Math.Pow(2, 24), "$2^{24}$"),
                new(0.1d, "0.1"),
                new(0.01d, "0.01"),
                new(123.456d, "123.456"),
                new(1d, "1"),
                new(-1d, "-1"),
                //new(0d, "0"),
            ];

            var t2 = GenerateTable(values, true);
            var t3 = GenerateTable(values, false);

            var table = t2 + Environment.NewLine + Environment.NewLine + t3;
        }
    }
}
