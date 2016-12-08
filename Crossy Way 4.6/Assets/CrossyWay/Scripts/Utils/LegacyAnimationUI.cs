using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LegacyAnimationUI : MonoBehaviour {

	// relationship between Animation components and their UI Graphics
	public class AnimatedGraphic {
		
		public Animation animation;
		public Graphic[] graphics;
		
		public AnimatedGraphic(Animation animation, Graphic[] graphics) {
			this.animation = animation;
			this.graphics = graphics;
		}
		
	}
	
	// list of relationships
	private List<AnimatedGraphic> m_AnimatedGraphics = new List<AnimatedGraphic>();
	
	void Awake() {
		// find all legacy animated UI elements
		Animation[] animations = GetComponentsInChildren<Animation>(true);
		foreach (Animation anim in animations) {
			Graphic[] graphics = anim.GetComponentsInChildren<Graphic>(true);
			
			if (graphics.Length > 0) {
				m_AnimatedGraphics.Add(new AnimatedGraphic(anim, graphics));
			}
		}
		
		if (m_AnimatedGraphics.Count == 0) {
			Debug.LogWarning("LegacyAnimationUI couldn't find any Legacy Animated UI components in hierarchy object", gameObject);
			enabled = false;
			return;
		}
	}
	
	void Update() {
		// set all dirty flags on any animating graphics
		foreach (AnimatedGraphic animatedGraphic in m_AnimatedGraphics) {
			if (animatedGraphic.animation.isPlaying) {
				foreach (Graphic graphic in animatedGraphic.graphics) {
					graphic.SetAllDirty();
					
					// NOTE - One of these may be all that's needed to animate color/alpha, but I made this script as simple and general-purpose as possible
					//graphic.SetMaterialDirty();
					//graphic.SetVerticesDirty();
				}
			}
		}
	}
}
