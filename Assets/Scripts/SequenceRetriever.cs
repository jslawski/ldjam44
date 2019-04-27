using System.Collections.Generic;
using UnityEngine;

/**
* This class retrieves a sequence scriptable object from
* our assets.
*/
public static class SequenceRetriever
{
  
  /// <param name="gameDifficulty">The difficulty the player is playing at.</param>
  /// <param name="previousSequence">The sequence the player just played at. We don't want to return the same sequence.</param>
  /// <returns>A sequence for the player to play or null if we can't find one.</returns>
  public static Sequence GetNextSequence(GameDifficulty gameDifficulty, Sequence previousSequence = null)
  {
    Object[] loadedSequencesForDifficulty = GetSequenceAssetsForGameDifficulty(gameDifficulty);
    Object[] prunedSequences = GetPrunedSequenceAssets(loadedSequencesForDifficulty, previousSequence);
    return PickSequence(prunedSequences);
  }

  /// <summary>
  /// We've divided our sequences by difficulty. This function gets all sequences
  /// for a given difficulty.
  /// </summary>
  /// <param name="gameDifficulty">The difficulty the player is playing at.</param>
  /// <returns>An array of all sequences, as objects, for the given difficulty level.</returns>
  private static Object[] GetSequenceAssetsForGameDifficulty(GameDifficulty gameDifficulty)
  {
    // Our Sequence assets are under Assets/Resources/Sequences/<Difficulty>/
    return Resources.LoadAll("Sequences/" + gameDifficulty.ToString());
  }

  /// <summary>
  /// This function prunes out the given sequence from the list of loaded sequences.
  /// </summary>
  /// <param name="loadedSequences">List of loaded sequences.</param>
  /// <param name="sequenceToRemove">The sequence we want pruned out.</param>
  /// <returns>A list of sequences that does not incllude the sequenceToRemove</returns>
  private static Object[] GetPrunedSequenceAssets(Object[] loadedSequences, Sequence sequenceToRemove)
  {
    List<Object> prunedSequences = new List<Object>();
    if (loadedSequences == null || loadedSequences.Length == 0)
    {
      return prunedSequences.ToArray();
    }

    int numSequences = loadedSequences.Length;
    for (int i = 0; i < numSequences; i++)
    {
      Sequence sequence = loadedSequences[i] as Sequence;
      if (sequence == sequenceToRemove)
      {
        continue;
      }

      prunedSequences.Add(loadedSequences[i]);
    }

    return prunedSequences.ToArray();
  }

  /// <summary>
  /// Picks a random sequence out of the array of loaded sequences.
  /// </summary>
  /// <param name="loadedSequences">All the eligible sequences to choose from.</param>
  /// <returns></returns>
  private static Sequence PickSequence(Object[] loadedSequences)
  {
    if (loadedSequences == null || loadedSequences.Length == 0)
    {
      return null;
    }

    if (loadedSequences.Length == 1)
    {
      return loadedSequences[0] as Sequence;
    }

    System.Random randomNumberGenerator = new System.Random();
    int sequenceIndex = randomNumberGenerator.Next(loadedSequences.Length);
    return loadedSequences[sequenceIndex] as Sequence;
  }
}
