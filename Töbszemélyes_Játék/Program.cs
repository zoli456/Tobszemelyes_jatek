using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

#region Keretrendszer

// Az AbsztraktÁllpot osztályt ki kellett bővíteni a GetHeurisztika metódussal.
// Egyébként megegyezik az előző fejezetben tárgyalt változattal.
internal abstract class AbsztraktÁllapot : ICloneable
{
    public virtual object Clone()
    {
        return MemberwiseClone();
    }

    public abstract bool ÁllapotE();
    public abstract bool CélÁllapotE();
    public abstract int OperátorokSzáma();
    public abstract bool SzuperOperátor(int i);

    public override bool Equals(object a)
    {
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    // Ez a metódus adja vissza, mennyire jó az adott állapot
    // Ez csak egy hook, felül kell írni, ha
    // kétszemélyes játékot vagy best-first algoritmus alkalmazunk,
    // vagy bármilyen más algoritmust, ami heurisztikán alapszik.
    // Más esetben, pl. backtrack esetén, nem kell felülírni.
    public virtual int GetHeurisztika()
    {
        return 0;
    }
}

// Az előző fejezetben ismertetett Csúcs osztályt bővítettük
// egy-két metódussal, amely a két személyes játékok megvalósításához kell.
internal class Csúcs
{
    private readonly AbsztraktÁllapot állapot;

    // A szülőkön túl a gyermekeket is tartalmazza a Csúcs osztály.
    private List<Csúcs> gyermekek = new List<Csúcs>();

    // Ez a mező tartalmazza, hogy melyik operátor segítségével jutottunk ebbe a csúcsba a szülő csúcsból.
    // Ennek segítségével tudom megmondani, melyik az ajánlott lépés NegaMaxMódszer esetén.
    private int melyikOperátorralJutottamIde = -1; // ha -1, akkor még nincs beállítva
    private readonly int mélység;
    private readonly Csúcs szülő;

    public Csúcs(AbsztraktÁllapot kezdőÁllapot)
    {
        állapot = kezdőÁllapot;
        mélység = 0;
        szülő = null;
    }

    public Csúcs(Csúcs szülő)
    {
        állapot = (AbsztraktÁllapot)szülő.állapot.Clone();
        mélység = szülő.mélység + 1;
        this.szülő = szülő;
    }

    // Erre a metódusra azért van szükség, hogy a kiterjesztés működjön a JátékCsúcsra is.
    protected virtual Csúcs createGyermekCsúcs(Csúcs szülő)
    {
        return new Csúcs(szülő);
    }

    public Csúcs GetSzülő()
    {
        return szülő;
    }

    public int GetMélység()
    {
        return mélység;
    }

    public bool TerminálisCsúcsE()
    {
        return állapot.CélÁllapotE();
    }

    public int OperátorokSzáma()
    {
        return állapot.OperátorokSzáma();
    }

    public bool SzuperOperátor(int i)
    {
        // megjegyzem, melyik operátorral jutottam ebbe az állapotba
        melyikOperátorralJutottamIde = i;
        return állapot.SzuperOperátor(i);
    }

    public override bool Equals(object obj)
    {
        var cs = (Csúcs)obj;
        return állapot.Equals(cs.állapot);
    }

    public override int GetHashCode()
    {
        return állapot.GetHashCode();
    }

    public override string ToString()
    {
        return állapot.ToString();
    }

    // Alkalmazza az összes alkalmazható operátort.
    // Visszaadja az így előálló új csúcsokat.
    public List<Csúcs> Kiterjesztés()
    {
        gyermekek = new List<Csúcs>();
        for (var i = 0; i < OperátorokSzáma(); i++)
        {
            // Új gyermek csúcsot készítek.
            // Ezzel a sorral nem működik a Kiterjesztés a JátékCsúcsban.
            // --- Csúcs újCsúcs = new Csúcs(this); ---
            // Ezért ezt használjuk:
            var újCsúcs = createGyermekCsúcs(this);
            // Kipróbálom az i.-dik alapoperátort. Alkalmazható?
            if (újCsúcs.SzuperOperátor(i))
                // Ha igen, hozzáadom az újakhoz.
                gyermekek.Add(újCsúcs);
        }

        return gyermekek;
    }

    // Visszaadja a csúcs heurisztikáját.
    // Ha saját heurisztikát akarunk írni, akkor azt a saját állapot osztályunkba kell megírni.
    public int GetHeurisztika()
    {
        return állapot.GetHeurisztika();
    }

    // Visszaadja melyik operátorral jutottunk ide.
    // Ezzel az int értékkel kell majd meghívni a SzuperOperátor-t.
    public int GetMelyikOperátorralJutottamIde()
    {
        return melyikOperátorralJutottamIde;
    }

    // Nyomkövetéshez hasznos.
    public void Kiir()
    {
        Console.WriteLine(this);
        foreach (var gyermek in gyermekek) gyermek.Kiir();
    }
} // A játék csúcs a csúcs osztály kibővítése egy heurisztika értékkel.

// Ezt a fajta csúcsot fel lehet használni a best first algoritmushoz is.
internal class JátékCsúcs : Csúcs
{
    private int mennyireJó = -1; // mennyire jó, ha -1, akkor még nincs beállítva

    // Konstruktor:
    // A belső állapotot beállítja a start csúcsra.
    // A hívó felelősége, hogy a kezdő állapottal hívja meg.
    // A start csúcs mélysége 0, szülője nincs.
    public JátékCsúcs(AbsztraktÁllapot kezdőÁllapot) :
        base(kezdőÁllapot)
    {
    }

    // Egy új gyermek csúcsot készít.
    // Erre még meg kell hívni egy alkalmazható operátor is, csak azután lesz kész.
    public JátékCsúcs(Csúcs szülő) : base(szülő)
    {
    }

    // Erre a metódusra azért van szükség, hogy a kiterjesztés
    // működjön a JátékCsúcsra is.
    protected override Csúcs createGyermekCsúcs(Csúcs szülő)
    {
        return new JátékCsúcs(szülő);
    }

    // Visszaadja a csúcshoz tartozó heurisztikát.
    // Ez a csúcsban lévő állapot heurisztikája
    // megszorozva a paraméterben megkapott szor értékkel.
    // NegaMax esetén a szor általában 1, ha az a játékos lép,
    // akinek jó lépést keresünk, -1, ha az ellenfél lép.
    // Mivel minden állapothoz csak egy heurisztika van, ami nem
    // veszi figyelembe, hogy ki lép, ezért a MiniMax-nak is
    // úgy kell használni ezt a metódust, mint a NegaMax-nak.
    public int GetMennyireJó(int szor)
    {
        if (mennyireJó == -1) mennyireJó = GetHeurisztika() * szor;
        return mennyireJó;
    }
}

// Ebből kell leszármaztatni a lépés ajánló algoritmusokat, mint a NegaMax módszer.
internal abstract class Stratégia
{
    // Ha a start csúcs zsákutca, akkor null-t ad vissza.
    // Egyébként azt a csúcsot, amibe a stratégia szerint érdemes lépni.
    public abstract JátékCsúcs MitLépjek(JátékCsúcs start);
} // Egy lépést ajánl valamely játékosnak a NegaMax módszer alapján.

// Ehhez előre tekint és a heurisztika meghatározása után megkeresi a legkedvezőbb utat a játék fában.
// Ha a játékfa levelei terminális csúcsok, akkor a legkedvezőbb út a nyerő stratégia lesz.
internal class NegaMaxMódszer : Stratégia
{
    private readonly int maxMélység; // Ennyi lépésre tekintünk előre.

    // Minél több lépésre tekintünk előre, annál intelligensebbnek tűnik a gép, hiszen jobb lépést választ.
    // Ezt a TicTacToe esetén figyelhetjük meg.
    // Ugyanakkor minél több lépést generálunk, annál lassabb lesz az algoritmus.
    // Ennek egy megoldása az Alfabéta-vágás, de ezt nem programoztuk le.
    public NegaMaxMódszer(int intelligencia)
    {
        maxMélység = intelligencia;
    }

    // Egy játékcsúcsot ad vissza.
    // Ennek a GetMelyikOperátorralJutottamIde() függvénye mondja meg,
    // melyik lépést ajánlja a NegaMax módszer.
    // Ha zsákutcában van, akkor null-t ad vissza.
    public override JátékCsúcs MitLépjek(JátékCsúcs start)
    {
        Csúcs levél = MaxLépés(start, start.GetMélység() + maxMélység);
        if (levél == start) return null;
        while (levél.GetSzülő() != start) levél = levél.GetSzülő();
        //levél.Kiir(); // Nyomkövetés esetén hasznos segítség
        return (JátékCsúcs)levél;
    }

    // Feltételezi, hogy a start csúcsban a kérdező játékos kérdezi, mit lépjen.
    // A kérdező játékos legjobb lépését, tehát a legnagyobb heurisztikájú
    // csúcs felé vezető lépést választja.
    // A gyermek csúcsok heurisztikáját NegaMax módszerrel számoljuk.
    private JátékCsúcs MaxLépés(JátékCsúcs start, int maxMélység)
    {
        var akt = start;
        if (akt.GetMélység() == maxMélység) return akt;
        if (akt.TerminálisCsúcsE()) return akt;
        List<Csúcs> gyermekek = null;
        gyermekek = akt.Kiterjesztés();
        if (gyermekek.Count == 0) return akt;
        var elsőGyermek = (JátékCsúcs)gyermekek[0];
        var leg = MinLépés(elsőGyermek, maxMélység);
        var h = leg.GetMennyireJó(+1);
        for (var i = 1; i < gyermekek.Count; i++)
        {
            var gyermek = (JátékCsúcs)gyermekek[i];
            var legE = MinLépés(gyermek, maxMélység);
            var hE = legE.GetMennyireJó(+1);
            if (hE > h)
            {
                h = hE;
                leg = legE;
            }
        }

        return leg;
    }

    // Felételezi, hogy a start csúcsban a a kérdező játékos ellenfele lép.
    // Az ellenfél játékos legjobb lépését választja, tehát azt,
    // ami a kérdező játékosnak a legrosszabb.
    // Egybe lehetne vonni a MaxLépéssel, hiszen csak 5 helyen más.
    // Ezek a sorokat megjelöltük.
    private JátékCsúcs MinLépés(JátékCsúcs start, int maxMélység)
    {
        var akt = start;
        if (akt.GetMélység() == maxMélység) return akt;
        if (akt.TerminálisCsúcsE()) return akt;
        List<Csúcs> gyermekek = null;
        gyermekek = akt.Kiterjesztés();
        if (gyermekek.Count == 0) return akt;
        var elsőGyermek = (JátékCsúcs)gyermekek[0];
        var leg = MaxLépés(elsőGyermek, maxMélység); //más
        var h = leg.GetMennyireJó(-1); // más
        for (var i = 1; i < gyermekek.Count; i++)
        {
            var gyermek = (JátékCsúcs)gyermekek[i];
            var legE = MaxLépés(gyermek, maxMélység); //más
            var hE = legE.GetMennyireJó(-1); //más
            if (hE < h)
            {
                h = hE;
                leg = legE;
            } //más
        }

        return leg;
    }
}

#endregion

// A Tic Tac Toe játék állapot osztálya.
internal class TicTacToeÁllapot : AbsztraktÁllapot
{
    private static readonly int N = 3; // 3 szor 3-mas tábla
    private int countX, countO; // X-ek és O-k száma

    private bool nyert; // nyertes állapt-e

    // NxN-es char mátrix
    // ' ': üres, 'X':első játékos jele, 'O': második játékos jele
    private char[,] tábla;
    private int üresekSzáma;

    public TicTacToeÁllapot()
    {
        tábla = new char[N, N];
        for (var i = 0; i < N; i++)
        for (var j = 0; j < N; j++)
            tábla[i, j] = ' ';

        countX = 0; // kezdetben egy X sincs
        countO = 0; // kezdetben egy O sincs
        nyert = false;
        üresekSzáma = N * N;
    }

    public override bool ÁllapotE()
    {
        return true;
    }

    public override bool CélÁllapotE()
    {
        return nyert || üresekSzáma == 0;
    }

    private bool preRak(int x, int y)
    {
        return x >= 0 && x < N && y >= 0 && y < N && tábla[x, y] == ' ';
    }

    private bool rak(int x, int y)
    {
        if (!preRak(x, y)) return false;
        char c;
        if (countX > countO) c = 'O';
        else c = 'X';
        tábla[x, y] = c;
        var régiNyert = nyert;
        nyert = // nem elég általános, csak N = 3-ra jó!
            (tábla[0, y] == c && tábla[1, y] == c && tábla[2, y] == c) ||
            (tábla[x, 0] == c && tábla[x, 1] == c && tábla[x, 2] == c);
        nyert = nyert || (x == y &&
                          tábla[0, 0] == c && tábla[1, 1] == c && tábla[2, 2] == c);
        nyert = nyert || (x + y == N - 1 &&
                          tábla[0, 2] == c && tábla[1, 1] == c && tábla[2, 0] == c);
        üresekSzáma--;
        if (c == 'X') countX++;
        else countO++;
        if (ÁllapotE()) return true;
        tábla[x, y] = ' '; // visszavonás
        nyert = régiNyert;
        üresekSzáma++;
        if (c == 'X') countX--;
        else countO--;
        return false;
    }

    public override int OperátorokSzáma()
    {
        return 9;
    }

    public override bool SzuperOperátor(int i)
    {
        switch (i)
        {
            case 0: return rak(0, 0);
            case 1: return rak(0, 1);
            case 2: return rak(0, 2);
            case 3: return rak(1, 0);
            case 4: return rak(1, 1);
            case 5: return rak(1, 2);
            case 6: return rak(2, 0);
            case 7: return rak(2, 1);
            case 8: return rak(2, 2);
            default: return false;
        }
    }

    // Ezt most felül kell írni, mert tömb típusú mezőnk is van.
    // Egy szűk területre kell koncentrálni.
    public override object Clone()
    {
        var új = new TicTacToeÁllapot();
        új.tábla = (char[,])tábla.Clone();
        új.countX = countX;
        új.countO = countO;
        új.nyert = nyert;
        új.üresekSzáma = üresekSzáma;
        return új;
    }

    public override bool Equals(object a)
    {
        var másik = (TicTacToeÁllapot)a;
        return tábla.Equals(másik.tábla);
    }

    public override int GetHashCode()
    {
        return tábla.GetHashCode();
    }

    // Ez a metódus adja vissza, mennyire jó az adott állapot.
    public override int GetHeurisztika()
    {
        if (nyert) return 100 * (3 * N + 1);
        // szabad sorok, oszlopok, és átlok száma
        // szabad a sor, ha csak egy fajta szimbolum van benne
        return szabad('X');
    }

    // Ez egy kicsit általánosra sikerült, hiszen csak
    // szabad('X') formában fogjuk hívni, habár hívható lenne
    // szabad('0') formában is. Ez akkor lesz hasznos, ha a
    // GetHeurisztika() függvényt át akarjuk írni.
    private int szabad(char c)
    {
        var count = 0;
        for (var i = 0; i < N; i++)
        {
            int sorX = 0, sorO = 0;
            int oszlopX = 0, oszlopO = 0;
            for (var j = 0; j < N; j++)
            {
                if (tábla[i, j] == 'X') sorX++;
                if (tábla[i, j] == 'O') sorO++;
                if (tábla[j, i] == 'X') oszlopX++;
                if (tábla[j, i] == 'O') oszlopO++;
            }

            if (c == 'X' && sorX > 0 && sorO == 0) count += sorX;
            if (c == 'O' && sorO > 0 && sorX == 0) count += sorO;
            if (c == 'X' && oszlopX > 0 && oszlopO == 0) count += oszlopX;
            if (c == 'O' && oszlopO > 0 && oszlopX == 0) count += oszlopO;
        }

        int átló1X = 0, átló1O = 0;
        int átló2X = 0, átló2O = 0;
        for (var i = 0; i < N; i++)
        {
            if (tábla[i, i] == 'X') átló1X++;
            if (tábla[i, i] == 'O') átló1O++;
            if (tábla[N - 1 - i, i] == 'X') átló2X++;
            if (tábla[N - 1 - i, i] == 'O') átló2O++;
        }

        if (c == 'X' && átló1X > 0 && átló1O == 0) count += átló1X;
        if (c == 'O' && átló1O > 0 && átló1X == 0) count += átló1O;
        if (c == 'X' && átló2X > 0 && átló2O == 0) count += átló2X;
        if (c == 'O' && átló2O > 0 && átló2X == 0) count += átló2O;
        return count;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < N; i++)
        {
            sb.Append('\n');
            for (var j = 0; j < N; j++)
            {
                sb.Append(tábla[i, j]);
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);
        }

        return sb.ToString();
    }
}

internal class Fejben21Állapot : AbsztraktÁllapot
{
    private static readonly int N = 21; // fejben 21, 21-ig kell menni
    private static readonly int K = 3; // max K-t lehet hozzáadni a számhoz
    private int szám;

    public Fejben21Állapot()
    {
        szám = 0;
    } // az első játékos 0-tól indul

    public override bool ÁllapotE()
    {
        return szám <= N;
    }

    public override bool CélÁllapotE()
    {
        return szám == N;
    }

    // Ennek a játéknak egyszerű nyerő stratégiája van.
    // A kezdő játékos nyer, ha 1-et mond, majd rendre:
    // 5, 9, 13, 17, 21.
    // Ha valaki 1-gyel kezd, akkor az ellenfél mondhat 2, 3, vagy 4-et.
    // Bármelyiket is mondja az ellenfél, a kezdő játékos mondhat 5-öt.
    // Így az 1, 5, 9, 13, 17, 21 sor tartható.
    // Hogy a rendszer megtalálja ezt a sorozatot, ezért a sorozat elemeire
    // 100-at ad a heurisztika, minden más értékre csak 1-et.
    public override int GetHeurisztika()
    {
        return szám % (K + 1) == 1 ? 100 : 1;
    }

    private bool preLép(int i)
    {
        return i >= 0 && i < K;
    }

    private bool lép(int i)
    {
        if (!preLép(i)) return false;
        i++; // ha i=0, akkor 1-et kell hozzáadni, stb..
        szám += i;
        if (ÁllapotE()) return true;
        szám -= i;
        return false;
    }

    // Itt szerencsére nem kellett belső switch-t írni.
    // Ez inkább kivételes eset.
    public override bool SzuperOperátor(int i)
    {
        return lép(i);
    }

    public override int OperátorokSzáma()
    {
        return K;
    }

    public override string ToString()
    {
        return szám.ToString();
    }
}

internal class RókaFogóÁllapot : AbsztraktÁllapot
{
    private const byte N = 8;
    private Point[] kutyák = new Point[4];
    private bool nyert; // nyertes állapt-e
    private Point róka;
    private bool teszt;

    public RókaFogóÁllapot()
    {
        róka = new Point(2, 0);
        kutyák[0] = new Point(1, 7);
        kutyák[1] = new Point(3, 7);
        kutyák[2] = new Point(5, 7);
        kutyák[3] = new Point(7, 7);
    }

    public override bool ÁllapotE()
    {
        if (róka.X < 0 || róka.Y < 0 || róka.X > 7 || róka.Y > 7) return false;
        if (kutyák.Any(x => x.X < 0 || x.Y < 0 || x.X > 7 || x.Y > 7)) return false;
        for (byte i = 0; i < kutyák.Length - 1; i++)
            if (kutyák.All(x => x.X == kutyák[i].X && x.Y == kutyák[i].Y))
                return false;

        if (kutyák.All(x => x.X == róka.X && x.Y == róka.Y)) return false;
        return true;
    }

    public override bool CélÁllapotE()
    {
        return kutyák.All(x => x.Y < róka.Y) || lepesekszama() == 0;
    }

    public override int OperátorokSzáma()
    {
        return 8;
    }

    private bool lep(sbyte index, sbyte x, sbyte y)
    {
        switch (index)
        {
            case 0:
                kutyák[index].X += x;
                kutyák[index].Y += y;
                break;
            case 1:
                kutyák[index].X += x;
                kutyák[index].Y += y;
                break;
            case 2:
                kutyák[index].X += x;
                kutyák[index].Y += y;
                break;
            case 3:
                kutyák[index].X += x;
                kutyák[index].Y += y;
                break;
            case 4:
                róka.X += x;
                róka.Y += y;
                break;
        }

        if (!ÁllapotE())
        {
            switch (index)
            {
                case 0:
                    kutyák[index].X -= x;
                    kutyák[index].Y -= y;
                    break;
                case 1:
                    kutyák[index].X -= x;
                    kutyák[index].Y -= y;
                    break;
                case 2:
                    kutyák[index].X -= x;
                    kutyák[index].Y -= y;
                    break;
                case 3:
                    kutyák[index].X -= x;
                    kutyák[index].Y -= y;
                    break;
                case 4:
                    róka.X -= x;
                    róka.Y -= y;
                    break;
            }

            return false;
        }

        return true;
    }

    public override int GetHeurisztika()
    {
        /* Console.WriteLine(kutyák[0] + " " + kutyák[1] + " " + kutyák[2] + " " + kutyák[3]);
         Console.Write(Math.Abs(lepesekszama() - 4) * 10 + " ");*/
        return Math.Abs(lepesekszama() - 4) * 10;
    }

    private byte lepesekszama()
    {
        var róka_temp = róka;
        byte lehetseges = 0;
        for (byte i = 8; i <= 11; i++)
        {
            róka = róka_temp;
            if (SzuperOperátor(i)) lehetseges++;
        }

        róka = róka_temp;
        return lehetseges;
    }

    public override bool SzuperOperátor(int i)
    {
        switch (i)
        {
            case 0: return lep(0, 1, -1);
            case 1: return lep(0, -1, -1);
            case 2: return lep(1, 1, -1);
            case 3: return lep(1, -1, -1);
            case 4: return lep(2, 1, -1);
            case 5: return lep(2, -1, -1);
            case 6: return lep(3, 1, -1);
            case 7: return lep(3, -1, -1);
            case 8: return lep(4, -1, 1);
            case 9: return lep(4, 1, 1);
            case 10: return lep(4, 1, -1);
            case 11: return lep(4, -1, -1);
            default: return false;
        }
    }

    public override string ToString()
    {
        var temp = "\n";
        for (byte Y = 0; Y < N; Y++)
        {
            for (byte X = 0; X < N; X++)
            {
                if (kutyák.Any(x => x.X == X && x.Y == Y))
                {
                    temp += "X ";
                    continue;
                }

                if (róka.X == X && róka.Y == Y)
                {
                    temp += "Y ";
                    continue;
                }

                temp += "0 ";
            }

            temp += "\n";
        }

        return temp;
    }

    public override object Clone()
    {
        var új = new RókaFogóÁllapot();
        új.kutyák = (Point[])kutyák.Clone();
        új.róka = róka;
        /*új.tábla = (char[,])tábla.Clone();
        új.countX = countX;
        új.countO = countO;
        új.nyert = nyert;
        új.üresekSzáma = üresekSzáma;*/
        return új;
    }

    public override int GetHashCode()
    {
        return kutyák.GetHashCode();
    }

    public override bool Equals(object a)
    {
        var másik = (RókaFogóÁllapot)a;
        return kutyák.Equals(másik.kutyák) && róka == róka;
    }
}

internal class KörSzámÁllapot : AbsztraktÁllapot
{
    private int szám;
    private int[] szamok = { 1, 5, 6, 4, 8, 6, 4, 3, 1, 2, 2, 8 };

    public KörSzámÁllapot()
    {
        szám = 0;
    }

    public override bool ÁllapotE()
    {
        return true;
    }

    public override bool CélÁllapotE()
    {
        //elfogytak-e a lépések
        return szamok.All(x => x == 0);
    }

    public override int GetHeurisztika()
    {
        return Math.Abs(szám - 50); /*szamok.Where(x => x == szamok.Max()).Sum();*/
    }

    private bool preLép(int i)
    {
        //A kezdésnél bármelyiket lehet választani
        if (szamok.All(x => x != 0)) return true;
        //amit már kitettek nem lehet újra kitenni
        if (szamok[i] == 0) return false;
        //kivétel1: ha utolsót szeretnénk kitenni és megvizsgáljuk a tömb első elemét
        if (szamok[0] == 0 && i == szamok.Length - 1) return true;
        //kivétel2: ha elsőt szeretnénk kitenni és megvizsgáljuk a tömb utolsó elemét
        if (szamok[szamok.Length - 1] == 0 && i == 0) return true;
        //Minden más esetben
        if (i != 0)
            if (szamok[i - 1] == 0)
                return true;
        if (i != szamok.Length - 1)
            if (szamok[i + 1] == 0)
                return true;
        ///////////////////////////
        return false;
    }

    private bool lép(int i)
    {
        if (!preLép(i)) return false;
        szám += szamok[i];
        if (!ÁllapotE()) return false;
        szamok[i] = 0;
        return true;
    }

    public override bool SzuperOperátor(int i)
    {
        return lép(i);
    }

    public override int OperátorokSzáma()
    {
        return 12;
    }

    public override string ToString()
    {
        var temp = "";
        for (byte i = 0; i < OperátorokSzáma(); i++) temp += szamok[i] + " ";
        return temp;
    }

    public override object Clone()
    {
        var új = new KörSzámÁllapot();
        új.szamok = (int[])szamok.Clone();
        új.szám = szám;
        return új;
    }

    public override bool Equals(object a)
    {
        var másik = (KörSzámÁllapot)a;
        return szamok.Equals(másik.szamok) && szám == másik.szám;
    }
}

// Egy játékot vezényel le.
// Ez egybevonható a főprogramból, hiszen abból emeltük ki.
internal class Játék
{
    private readonly AbsztraktÁllapot startÁllapot;
    private readonly Stratégia strat;

    public Játék(AbsztraktÁllapot startÁllapot, Stratégia strat)
    {
        this.startÁllapot = startÁllapot;
        this.strat = strat;
    }

    public void start()
    {
        int[] szamok = { 1, 5, 6, 4, 8, 6, 4, 3, 1, 2, 2, 8 };
        var valaszottak = new List<int>();
        var valaszottak2 = new List<int>();
        var pont = 0;
        var pont2 = 0;
        var akt = new JátékCsúcs(startÁllapot);
        Console.WriteLine("Játszhat a gép ellen!");
        while (!akt.TerminálisCsúcsE())
        {
            // Ellenfél lépése.
            Console.WriteLine("Jelenlegi állás: {0}", akt);
            akt = strat.MitLépjek(akt);
            var i = akt.GetMelyikOperátorralJutottamIde();
            pont2 += szamok[i];
            valaszottak2.Add(szamok[i]);
            Console.WriteLine("A gép ezt az operátort választotta: {0}", i);
            // Nyertes állás?
            if (akt.TerminálisCsúcsE()) break;
            // Saját lépésem.
            var b = false;
            while (!b)
            {
                Console.WriteLine("Jelenlegi állás: {0}", akt);
                Console.WriteLine("Melyik operátort választja? (0,..,{0}): ", akt.OperátorokSzáma() - 1);
                var k = 0;
                try
                {
                    k = int.Parse(Console.ReadLine());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Hiba: {0}", e);
                    Console.WriteLine("Érvénytelen lépés. Újra!");
                    continue;
                }

                b = akt.SzuperOperátor(k);
                if (!b)
                {
                    Console.WriteLine("Ez az operátor nem alkalmazható. Újra!");
                }
                else
                {
                    pont += szamok[k];
                    valaszottak.Add(szamok[k]);
                }
            }
        }

        Console.WriteLine("Jelenlegi állás: {0}", akt);
        Console.WriteLine("A játéknak vége. Pontszámod: {0} Ellenfél pontszáma: {1}", pont, pont2);
        Console.Write("Te lépései:");
        for (var i = 0; i < valaszottak.Count; i++) Console.Write(valaszottak[i] + " ");
        Console.WriteLine();
        Console.Write("Gép lépései:");
        for (var i = 0; i < valaszottak.Count; i++) Console.Write(valaszottak2[i] + " ");
    }
} // Főprogram.

internal class Program
{
    private static void Main(string[] args)
    {
        Stratégia strat = new NegaMaxMódszer(9); // 5 mélységbe tekint előre
        /* AbsztraktÁllapot startFejben21Állapot = new KörSzámÁllapot();
                                                  Játék fejben21 = new Játék(startFejben21Állapot, strat);
                                                  Console.WriteLine("A Fejben 21 játék kezdetét veszi!");
                                                  fejben21.start();
                                                  Console.ReadLine();
                                                  AbsztraktÁllapot startTTTÁllapot = new TicTacToeÁllapot();
                                                  Játék tictactoe = new Játék(startTTTÁllapot, strat);
                                                  Console.WriteLine("A Tic Tac Toe játék kezdődik!");
                                                  tictactoe.start();*/
        /*AbsztraktÁllapot startTTTÁllapot = new RókaFogóÁllapot();
        Játék rókajáték = new Játék(startTTTÁllapot, strat);
        Console.WriteLine("Rókafogó játék kezdődik!");
        rókajáték.start();*/
        AbsztraktÁllapot startTTTÁllapot = new KörSzámÁllapot();
        var körszámállapot = new Játék(startTTTÁllapot, strat);
        Console.WriteLine("Körszám játék kezdődik!");
        körszámállapot.start();
        Console.ReadLine();
        Console.ReadLine();
    }
}