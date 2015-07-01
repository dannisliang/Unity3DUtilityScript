using UnityEngine;
using System.Collections;

/***
 * 屏幕尺,用于开发阶段在屏幕上画标准线,核对 UI 位置等操作
 * 用法:
 * - 将此脚本贴到相机 Camera 上
 * - 在 Inspetor 中设置 lines 的起点&终点参数[0,1]
 * */
#if UNITY_EDITOR
namespace EF4Debug
{
	[System.Serializable]
	public struct ScreenRulerLine
	{
		public Vector2 start;
		public Vector2 end;
	}

	[ExecuteInEditMode]
	public class ScreenRuler : MonoBehaviour
	{
		// 指定起点&终点,作线段
		public ScreenRulerLine[] lines;

		private static Material lineMaterial;

		void Start ()
		{

		}

		void OnPostRender ()
		{
			CreateLineMaterial ();
			lineMaterial.SetPass (0);
			DrawSquare ();
			DrawSreenSplitLine (); 
			DrawSpecifiedLine ();
		}

		private static void CreateLineMaterial ()
		{
			if (!lineMaterial) {
				lineMaterial = new Material ("Shader \"Lines/Colored Blended\" {" + "SubShader { Pass { " + "    Blend SrcAlpha OneMinusSrcAlpha " + "    ZWrite Off Cull Off Fog { Mode Off } " + "    BindChannels {" + "      Bind \"vertex\", vertex Bind \"color\", color }" + "} } }");
				lineMaterial.hideFlags = HideFlags.HideAndDontSave;
				lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void DrawSpecifiedLine(){
			/**
			 * 在屏幕内指定起点和重点画线,(x,y)
			 * 屏幕左下角为 (0,0) ,右上角为 (1,1)
			 * */
			if (lines != null && lines.Length > 0) {
				GL.PushMatrix ();//保存摄像机变换矩阵  
				GL.LoadPixelMatrix ();//设置用屏幕坐标绘图  
				Color co = Color.red;
				int width = Screen.width;
				int height = Screen.height;
				for(int i = 0 ; i < lines.Length ; i++){
					Vector2 s = lines [i].start;
					Vector2 e = lines [i].end;
					GL.Begin (GL.LINES);  
					GL.Color (co);  
					GL.Vertex3 (width * s.x, height * s.y, 0);  
					GL.Vertex3 (width * e.x, height * e.y, 0);  
					GL.End ();
				}
				GL.PopMatrix ();//还原 
			}
		}
		private void DrawSquare ()
		{
			GL.PushMatrix ();//保存摄像机变换矩阵  
			GL.LoadPixelMatrix ();//设置用屏幕坐标绘图  
			Color co = Color.red;

			Vector3 center = new Vector3 (Screen.width * 0.5f, Screen.height * 0.5f, 0);
			float halfWidth = 100 * 0.5f;
			float halfHeight = 100 * 0.5f;

			//上
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (center.x - halfWidth, center.y + halfHeight, 0);  
			GL.Vertex3 (center.x + halfWidth, center.y + halfHeight, 0);  
			GL.End (); 
			//下
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (center.x - halfWidth, center.y - halfHeight, 0);  
			GL.Vertex3 (center.x + halfWidth, center.y - halfHeight, 0);  
			GL.End ();
			//左
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (center.x - halfWidth, center.y + halfHeight, 0);  
			GL.Vertex3 (center.x - halfWidth, center.y - halfHeight, 0);  
			GL.End ();  
			//右
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (center.x + halfWidth, center.y + halfHeight, 0);  
			GL.Vertex3 (center.x + halfWidth, center.y - halfHeight, 0);  
			GL.End (); 

			GL.PopMatrix ();//还原 
		}

		/// <summary>
		/// 以屏幕中心为交点,画一个 十字形
		/// </summary>
		private void DrawSreenSplitLine ()
		{
			GL.PushMatrix ();//保存摄像机变换矩阵  

			GL.LoadPixelMatrix ();//设置用屏幕坐标绘图 
			Color co = Color.red;

			int x = Screen.width;
			int y = Screen.height;

			// 分隔屏幕水平线
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (0, y * 0.5f, 0);  
			GL.Vertex3 (x, y * 0.5f, 0);  
			GL.End (); 
			// 分隔屏幕垂直线
			GL.Begin (GL.LINES);  
			GL.Color (co);  
			GL.Vertex3 (x * 0.5f, 0, 0);  
			GL.Vertex3 (x * 0.5f, y, 0);  
			GL.End ();

			GL.PopMatrix ();//还原  
		}
	}
}
#endif

