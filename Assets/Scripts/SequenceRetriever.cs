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
  public static Sequence GetNextSequence(GameDifficulty gameDifficulty, Sequence previousSequence)
  {
    Object[] loadedSequencesForDifficulty = GetSequenceAssetsForGameDifficulty(gameDifficulty);
    Sequence sequence = PickSequence(loadedSequencesForDifficulty, previousSequence);
    if (sequence == null) { return null; }
    return sequence;
  }

  /// <summary>
  /// We've divided our sequences by difficulty. This function gets all sequences
  /// for a given difficulty.
  /// </summary>
  /// <param name="gameDifficulty">The difficulty the player is playing at.</param>
  /// <returns>An array of all sequences, as objects, for the given difficulty level.</returns>
  private static Object[] GetSequenceAssetsForGameDifficulty(GameDifficulty gameDifficulty)
  {
    string assetPath = "Assets/Resources/Sequences/" + gameDifficulty.ToString();
    return Resources.LoadAll(assetPath);
  }

  /// <summary>
  /// Picks a random sequence out of the array of loaded sequences while
  /// making sure that we don't reload the sequence the user just played at.
  /// </summary>
  /// <param name="loadedSequences">All the eligible sequences to choose from.</param>
  /// <param name="previousSequence">The sequence the player just played.</param>
  /// <returns></returns>
  private static Sequence PickSequence(Object[] loadedSequences, Sequence previousSequence)
  {
    return null;
  }
}
