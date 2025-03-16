using Proto;
using System.Collections.Generic;
namespace GeTPlanModel
{
    public class GeTAtom
    {
        public string? Symbol { get; private set; }
        public long? IntValue { get; private set; }
        public GeTReal? RealValue { get; private set; }
        public bool? BooleanValue { get; private set; }

        public GeTAtom(string? symbol = null, long? intValue = null, GeTReal? realValue = null, bool? booleanValue = null)
        {
            Symbol = symbol;
            IntValue = intValue;
            RealValue = realValue;
            BooleanValue = booleanValue;
        }

        // Display the atom in a string format
        public override string ToString()
        {
            if (Symbol != null) return $"{Symbol}";
            if (IntValue.HasValue) return $"{IntValue.Value}";
            if (RealValue != null) return $"{RealValue.ToString()}";
            if (BooleanValue.HasValue) return $"{BooleanValue.Value}";
            return "Empty Atom";
        }

        public object? GetValue()
        {
            if (IntValue.HasValue) return IntValue.Value;
            if (RealValue != null) return RealValue.ToDouble();
            if (BooleanValue.HasValue) return BooleanValue.Value;
            return null;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GeTAtom other)) return false;

            return Symbol == other.Symbol &&
                   IntValue == other.IntValue &&
                   (RealValue?.Equals(other.RealValue) ?? other.RealValue == null) &&
                   BooleanValue == other.BooleanValue;
        }

        public override int GetHashCode()
        {
            int hash = Symbol?.GetHashCode() ?? 0;
            hash = (hash * 397) ^ IntValue.GetHashCode();
            hash = (hash * 397) ^ (RealValue?.GetHashCode() ?? 0);
            hash = (hash * 397) ^ BooleanValue.GetHashCode();
            return hash;
        }
    }

    // A simple model for representing the Real number (fraction)
    public class GeTReal
    {
        public long Numerator { get; set; }
        public long Denominator { get; set; }

        public GeTReal(long numerator, long denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public GeTReal(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public double ToDouble()
        {
            return (double)Numerator / Denominator;
        }

        public override string ToString()
        {
            return $"{ToDouble().ToString()}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            GeTReal other = (GeTReal)obj;

            return Numerator == other.Numerator && Denominator == other.Denominator;
        }

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() + Denominator.GetHashCode();
        }
    }


    public class AtomModelFactory : IGeTModelFactory<GeTAtom, Atom> , IProtoFactory<GeTAtom, Atom>
    {
        public static GeTAtom Create(string symbol)
        {
            return new GeTAtom(symbol: symbol);
        }

        public static GeTAtom Create(long intValue)
        {
            return new GeTAtom(intValue: intValue);
        }

        public static GeTAtom Create(double realValue)
        {
            return new GeTAtom(realValue: RealModelFactory.Create(realValue));
        }

        public static GeTAtom Create(bool booleanValue)
        {
            return booleanValue ? TRUE() : FALSE();
        }

        public static GeTAtom Create(object value)
        {
            if (value is string stringValue)
            {
                return Create(stringValue);
            }
            if (value is int intValue)
            {
                return Create(intValue);
            }
            if (value is float realValue)
            {
                return Create(realValue);
            }
            if (value is bool booleanValue)
            {
                return Create(booleanValue);
            }
            return NONE();
        }

        public static GeTAtom TRUE()
        {
            return new GeTAtom(booleanValue: true);
        }

        public static GeTAtom FALSE()
        {
            return new GeTAtom(booleanValue: false);
        }

        public static GeTAtom NONE()
        {
            return new GeTAtom();
        }

        public GeTAtom FromProto(Atom atom)
        {
            if (atom == null)
            {
                return new GeTAtom(); // Return empty if no atom is given
            }

            switch (atom.ContentCase)
            {
                case Atom.ContentOneofCase.Symbol:
                    return new GeTAtom(symbol: atom.Symbol);
                case Atom.ContentOneofCase.Int:
                    return new GeTAtom(intValue: atom.Int);
                case Atom.ContentOneofCase.Real:
                    return new GeTAtom(realValue: new GeTReal(atom.Real.Numerator, atom.Real.Denominator));
                case Atom.ContentOneofCase.Boolean:
                    return atom.Boolean == true ? TRUE() : FALSE();
                default:
                    return NONE();
            }
        }

        public Atom ToProto(GeTAtom model)
        {
            return new Atom
            {
                Symbol = model.Symbol,
                Int = model.IntValue ?? 0,
                Real = model.RealValue == null ? null : new Real
                {
                    Numerator = model.RealValue.Numerator,
                    Denominator = model.RealValue.Denominator
                },
                Boolean = model.BooleanValue ?? false
            };
        }
    }

    public class GeTRealComparer : IComparer<GeTReal>
    {
        public int Compare(GeTReal x, GeTReal y)
        {
            // Compare by converting to double for simplicity
            return x.ToDouble().CompareTo(y.ToDouble());
        }
    }

    public class RealModelFactory : IGeTModelFactory<GeTReal, Real>, IProtoFactory<GeTReal, Real>
    {
        public static GeTReal Create(int numerator, int denominator)
        {
            return new GeTReal(numerator, denominator);
        }

        public static GeTReal Create(double value)
        {
            long denominator = 1;
            while (value % 1 != 0)
            {
                value *= 10;
                denominator *= 10;
            }

            return new GeTReal((long)value, denominator);
        }

        public GeTReal FromProto(Real real)
        {
            return new GeTReal(real.Numerator, real.Denominator);
        }

        public Real ToProto(GeTReal model)
        {
            return new Real
            {
                Numerator = model.Numerator,
                Denominator = model.Denominator
            };
        }
    }
}
