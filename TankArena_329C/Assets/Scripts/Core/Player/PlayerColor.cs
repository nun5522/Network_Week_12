using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [SerializeField] private TeamColorLookup teamColorLookup;
    [SerializeField] private TankPlayer player;
    [SerializeField] private SpriteRenderer[] playerSprites;
    
    [SerializeField] private int colorIndex;

    private void Start()
    {
        HandlePlayerColorChanged(0, player.PlayerColorIndex.Value);
        HandleTeamChanged(0, player.TeamIndex.Value);
        player.PlayerColorIndex.OnValueChanged += HandlePlayerColorChanged;
        player.TeamIndex.OnValueChanged += HandleTeamChanged;
    }
    
    private void HandlePlayerColorChanged(int oldIndex,int newIndex)
    {
        Debug.Log($"Color Changed : {newIndex}");
        colorIndex = newIndex;
        foreach(SpriteRenderer sprite in playerSprites)
        {
            if(teamColorLookup.GetTeamColor(colorIndex) != null)
            {
                sprite.color = (Color) teamColorLookup.GetTeamColor(colorIndex);
            }            
        }
    }

    public void HandleTeamChanged(int oldTeamIndex,int newTeamIndex)
    {
        Debug.Log($"Team Changed : {newTeamIndex}");
        if(teamColorLookup.GetTeamColor(newTeamIndex) == null)
        {
            return;
        }

        Color teamColor = (Color) teamColorLookup.GetTeamColor(newTeamIndex);

        foreach (SpriteRenderer sprite in playerSprites)
        {            
            sprite.color = teamColor;            
        }
    }

    private void OnDestroy()
    {
        player.PlayerColorIndex.OnValueChanged -= HandlePlayerColorChanged;
    }
}