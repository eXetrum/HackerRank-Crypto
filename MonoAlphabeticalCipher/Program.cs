using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Solution
{
    public struct Character
    {
        public char character;
        public bool IsRecovered;
    }
    public class Word
    {
        public Word(string word)
        {
            letters = new Character[word.Length];
            for (int i = 0; i < word.Length; ++i)
            {
                letters[i].character = word[i];
                letters[i].IsRecovered = false;
            }
        }
        public bool isValid()
        {
            foreach (var c in letters)
                if (!c.IsRecovered) return false;
            return true;
        }
        public override string ToString()
        {
            string msg = string.Empty;
            foreach (var l in letters)
                msg += (l.IsRecovered) ? l.character : '^';

            return msg;
        }
        public Character[] letters { get; set; }
    }

    static bool RecoverWord(ref Word word, ref Dictionary<int, List<string>> goodWords, ref Dictionary<char, char> decryptAlphabet)
    {
        List<string> candidats = goodWords[word.letters.Length];
        int validLetters = 0;
        int wordLen = word.letters.Length;
        foreach (var v in word.letters)
            if (v.IsRecovered) validLetters++;

        List<string> mathcedCandidats = new List<string>();

        for (int i = 0; i < candidats.Count; ++i)
        {
            string candidat = candidats[i];
            int charEq = 0;
            for (int j = 0; j < wordLen; ++j)
            {
                if (word.letters[j].IsRecovered && word.letters[j].character.Equals(candidat[j]))
                {
                    charEq++;
                }
            }
            if (charEq == validLetters)
                mathcedCandidats.Add(candidat);
        }
        bool alphabetChanged = false;
        if (mathcedCandidats.Count == 1)
        {
            string w = mathcedCandidats[0];
            for (int i = 0; i < wordLen; ++i)
            {
                if (word.letters[i].IsRecovered == false)
                {
                    decryptAlphabet[word.letters[i].character] = w[i];
                    word.letters[i].character = w[i];
                    word.letters[i].IsRecovered = true;
                    alphabetChanged = true;
                }
            }
            candidats.Remove(w);
            goodWords[wordLen] = candidats;
        }
        return alphabetChanged;
    }



    static void Main(String[] args)
    {

        string cipher = File.ReadAllText("cipher.txt");//Console.ReadLine();
        string word = string.Empty;
        string plain = string.Empty;
        System.IO.StreamReader file = new System.IO.StreamReader("dictionary.lst");
        // Частоты символов всех имеющихся расшифрованных слов
        Dictionary<char, double> plainFreq = new Dictionary<char, double>();
        // Частоты символов шифра
        Dictionary<char, double> cipherFreq = new Dictionary<char, double>();
        // Собираем слова одной длинны для всех имеющихся расшифрованных слов
        Dictionary<int, List<string>> plainLenCount = new Dictionary<int, List<string>>();
        // Собираем слова одной длинны для шифротекста
        Dictionary<int, List<string>> cipherLenCount = new Dictionary<int, List<string>>();
        int maxPlain = 0;
        while ((word = file.ReadLine()) != null)
        {
            word = word.ToLower();
            plain += word;
            int len = word.Length;
            if (maxPlain < len) maxPlain = len;
            if (!plainLenCount.ContainsKey(len))
                plainLenCount[len] = new List<string>();
            plainLenCount[len].Add(word);

            for (int i = 0; i < word.Length; ++i)
            {
                if (plainFreq.ContainsKey(word[i]))
                    plainFreq[word[i]]++;
                else
                    plainFreq[word[i]] = 1;
            }
        }
        file.Close();
        string[] words = cipher.Split();

        List<Word> workWords = new List<Word>();
        int maxCipher = 0;
        for (int w = 0; w < words.Length; ++w)
        {
            words[w] = words[w].ToLower();
            workWords.Add(new Word(words[w]));

            int len = words[w].Length;
            if (maxCipher < len) maxCipher = len;
            if (!cipherLenCount.ContainsKey(len))
                cipherLenCount[len] = new List<string>();
            cipherLenCount[len].Add(words[w]);

            for (int i = 0; i < len; ++i)
            {
                if (cipherFreq.ContainsKey(words[w][i]))
                    cipherFreq[words[w][i]]++;
                else
                    cipherFreq[words[w][i]] = 1;
            }
        }

        for (int i = 0; i < plainFreq.Keys.Count; ++i)
        {
            char key = plainFreq.Keys.ElementAt(i);
            plainFreq[key] = Convert.ToDouble((double)plainFreq[key] / plain.Length);
        }
        for (int i = 0; i < cipherFreq.Keys.Count; ++i)
        {
            char key = cipherFreq.Keys.ElementAt(i);
            cipherFreq[key] = Convert.ToDouble((double)cipherFreq[key] / cipher.Length);
        }

        /*Console.WriteLine("cipher freq:");
        foreach (var c in cipherFreq)
            Console.WriteLine(c.Key + " " + c.Value);*/

        Dictionary<char, char> decryptAlphabet = new Dictionary<char, char>();
        foreach (var c in cipherFreq)
            if (!decryptAlphabet.ContainsKey(c.Key))
                decryptAlphabet.Add(c.Key, '*');
        for (int w = 0; w < workWords.Count; ++w)
        {
            int len = workWords[w].letters.Length;
            if (len == maxCipher)
            {
                List<string> candidats = plainLenCount[len];
                string msg = string.Empty;
                //double encP = 0;
                foreach (var m in workWords[w].letters)
                {
                    msg += m.character;
                    //encP += cipherFreq[m.character];
                }
                Dictionary<double, string> probability = new Dictionary<double, string>();
                for (int i = 0; i < candidats.Count; ++i)
                {
                    double P = 0;
                    for (int j = 0; j < len; ++j)
                    {
                        double decP = plainFreq[candidats[i][j]];
                        double encP = cipherFreq[workWords[w].letters[j].character];
                        double diff = Math.Abs(decP - encP);
                        P += diff;
                    }
                    //Console.WriteLine("[{0} = {1}] = {2}", msg, candidats[i], P);
                    if (!probability.ContainsKey(P))
                        probability.Add(P, candidats[i]);

                }
                //Console.WriteLine("posible subtitution for {0} == {1}", msg, );
                string posibleWord = probability[probability.Keys.Min()];
                for (int i = 0; i < len; ++i)
                {
                    decryptAlphabet[workWords[w].letters[i].character] = posibleWord[i];
                    workWords[w].letters[i].character = posibleWord[i];
                    workWords[w].letters[i].IsRecovered = true;

                }
            }
        }

        for (int i = 0; i < workWords.Count; ++i)
        {
            Word w = workWords[i];
            for (int j = 0; j < w.letters.Length; ++j)
            {
                //Console.WriteLine(w.letters[j].character);
                if (!w.letters[j].IsRecovered && !decryptAlphabet[w.letters[j].character].Equals('*'))
                {
                    w.letters[j].character = decryptAlphabet[w.letters[j].character];
                    w.letters[j].IsRecovered = true;
                }
            }
            List<string> candidats = plainLenCount[w.letters.Length];
            if (RecoverWord(ref w, ref plainLenCount, ref decryptAlphabet)) i = -1;
            //Console.WriteLine(decryptAlphabet);
        }

        for (int i = 0; i < workWords.Count; ++i)
        {
            string str = workWords[i].ToString();
            if (str.Contains("^"))
                Console.Write("[{0}]", str);
            else
                Console.Write((i > 0 ? " " : "") + str);
        }
    }
}