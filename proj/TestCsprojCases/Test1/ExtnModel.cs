using Test1.Models;
namespace TestCsprojCases.Test1;


public static partial class ExtnUserModel{
	extension(UserModel z){
		/// <summary>
		/// 取得用戶的顯示名稱，若名稱為空則顯示 "Unknown"
		/// </summary>
		public string DisplayName => string.IsNullOrEmpty(z.Name) ? "Unknown" : z.Name;

		/// <summary>
		/// 取得用戶的 Email 域名
		/// </summary>
		public string EmailDomain {
			get {
				var atIndex = z.Email.IndexOf('@');
				return atIndex >= 0 ? z.Email.Substring(atIndex + 1) : string.Empty;
			}
		}

		/// <summary>
		/// 判斷用戶是否為管理員（ID 為 1）
		/// </summary>
		public bool IsAdmin => z.Id == 1;

		/// <summary>
		/// 格式化用戶資訊為字串
		/// </summary>
		public string FormatInfo() => $"User[{z.Id}]: {z.Name} ({z.Email})";

		/// <summary>
		/// 驗證 Email 格式是否有效
		/// </summary>
		public bool HasValidEmail() {
			return !string.IsNullOrEmpty(z.Email) && z.Email.Contains('@') && z.Email.Contains('.');
		}

		/// <summary>
		/// 取得用戶名稱的首字母
		/// </summary>
		public char GetInitial() {
			return string.IsNullOrEmpty(z.Name) ? '?' : z.Name[0];
		}

		/// <summary>
		/// 清空用戶資料
		/// </summary>
		public void Clear() {
			z.Name = string.Empty;
			z.Email = string.Empty;
		}

		/// <summary>
		/// 建立用戶的淺拷貝
		/// </summary>
		public UserModel Clone() {
			return new UserModel(z.Id, z.Name, z.Email);
		}

		/// <summary>
		/// 將用戶轉換為字典
		/// </summary>
		public Dictionary<string, object> ToDictionary() {
			return new Dictionary<string, object> {
				["Id"] = z.Id,
				["Name"] = z.Name,
				["Email"] = z.Email
			};
		}
	}
}

