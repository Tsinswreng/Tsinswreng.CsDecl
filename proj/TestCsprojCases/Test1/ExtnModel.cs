using Test1.Models;
namespace TestCsprojCases.Test1;


public static partial class ExtnUserModel{
	extension(UserModel z){
		/// 取得用戶的顯示名稱，若名稱為空則顯示 "Unknown"
		public string DisplayName => string.IsNullOrEmpty(z.Name) ? "Unknown" : z.Name;

		/// 取得用戶的 Email 域名
		public string EmailDomain {
			get {
				var atIndex = z.Email.IndexOf('@');
				return atIndex >= 0 ? z.Email.Substring(atIndex + 1) : string.Empty;
			}
		}

		/// 判斷用戶是否為管理員（ID 為 1）
		public bool IsAdmin => z.Id == 1;

		/// 格式化用戶資訊為字串
		public string FormatInfo() => $"User[{z.Id}]: {z.Name} ({z.Email})";

		/// 驗證 Email 格式是否有效
		public bool HasValidEmail() {
			return !string.IsNullOrEmpty(z.Email) && z.Email.Contains('@') && z.Email.Contains('.');
		}

		/// 取得用戶名稱的首字母
		public char GetInitial() {
			return string.IsNullOrEmpty(z.Name) ? '?' : z.Name[0];
		}

		/// 清空用戶資料
		public void Clear() {
			z.Name = string.Empty;
			z.Email = string.Empty;
		}

		/// 建立用戶的淺拷貝
		public UserModel Clone() {
			return new UserModel(z.Id, z.Name, z.Email);
		}

		/// 將用戶轉換為字典
		public Dictionary<string, object> ToDictionary() {
			return new Dictionary<string, object> {
				["Id"] = z.Id,
				["Name"] = z.Name,
				["Email"] = z.Email
			};
		}
	}
}

