using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TypingUtils
{
    private static string fullAA = "";
    private static string fullAB = "";
    private static int countNbAlignment = 0;

    public static float WordsPerMinute(string transcribedText, float timing) {
        return (transcribedText.Length - 1) / timing * 12f; // 60 * 1/5
    }


    public static int costOfSubstitution(char a, char b) {
        return a == b ? 0 : 1;
    }

    public static int LeveinshteinDistance(string x, string y) {
        return LeveinshteinMatrix(x, y)[x.Length, y.Length];
    }

    public static int[,] LeveinshteinMatrix(string x, string y) {
        int[,] dp = new int[x.Length + 1, y.Length + 1];

        for (int i = 0; i <= x.Length; i++) {
            for (int j = 0; j <= y.Length; j++) {
                if (i == 0) {
                    dp[i, j] = j;
                } else if (j == 0) {
                    dp[i, j] = i;
                } else {
                    dp[i, j] = Mathf.Min(dp[i - 1, j - 1] 
                    + costOfSubstitution(x[i - 1], y[j - 1]), 
                    dp[i - 1, j] + 1, 
                    dp[i, j - 1] + 1);
                }
            }
        }
        return dp;
    }

    public static float ErrorRate(string presentedText, string transcribedText)
    {
        float errorRate = TypingUtils.LeveinshteinDistance(presentedText, transcribedText) / TypingUtils.MeanSizeAlignments(
            presentedText, 
            transcribedText,
            TypingUtils.LeveinshteinMatrix(presentedText, transcribedText), 
            presentedText.Length, 
            transcribedText.Length, 
            "",
            "") * 100f;
        return errorRate;
    }

    public static float MeanSizeAlignments(string A, string B, int[,] D, int X, int Y, string AA, string AB) {    
        Align(A, B, D, X, Y, AA, AB);
        return fullAA.Length / countNbAlignment;
    }

    public static void Align(string A, string B, int[,] D, int X, int Y, string AA, string AB) {
        if (X == 0 && Y == 0) {
            fullAA += AA;
            fullAB += AB;
            countNbAlignment += 1;
            return;
        }
        
        if (X > 0 && Y > 0) {
            if (D[X,Y] == D[X-1,Y-1] && A[X-1] == B[Y-1])
            Align(A, B, D, X-1, Y-1, A[X-1] + AA, B[Y-1] + AB);
            if (D[X,Y] == D[X-1,Y-1] + 1)
            Align(A, B, D, X-1, Y-1, A[X-1] + AA, B[Y-1] + AB);
        }
        if (X > 0 && D[X,Y] == D[X-1,Y] + 1)
            Align(A, B, D, X-1, Y, A[X-1] + AA, "-" + AB);
        if (Y > 0 && D[X,Y] == D[X,Y-1] + 1)
            Align(A, B, D, X, Y-1, "-" + AA, B[Y-1] + AB);
        return;
    }
}
