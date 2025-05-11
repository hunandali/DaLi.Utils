' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	产生随机字符串
'
' 	name: Helper.RandomHelper
' 	create: 2019-03-13
' 	memo: 产生随机字符串
' 	
' ------------------------------------------------------------

Imports System.Text

Namespace Helper

	''' <summary>产生随机字符串</summary> 
	Public NotInheritable Class RandomHelper

#Region "常量"

		Private Const LOWER_CHARS As String = "abcdefghijklmnopqrstuvwxyz"
		Private Const UPPER_CHARS As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
		Private Const NUMBER_CHARS As String = "0123456789"

		''' <summary>中文姓名</summary>
		Public Shared NAMES_CHINESE As String() = {"安道全", "安陵容", "白富美", "白胜", "班淑娴", "鲍旭", "毕加索", "邴原", "补锅匠", "步骘", "蔡福", "蔡和", "蔡瑁", "蔡庆", "蔡文姬", "蔡中", "曹昂", "曹操", "曹纯", "曹芳", "曹洪", "曹奂", "曹髦", "曹丕", "曹仁", "曹睿", "曹爽", "曹休", "曹云奇", "曹彰", "曹真", "曹正", "曹植", "柴进", "常遇春", "车夫", "陈达", "陈登", "陈宫", "陈珪", "陈琳", "陈武", "陈禹", "陈震", "程秉", "程灵素", "程英", "程昱", "楚留箱", "慈恩", "崔牛皮", "崔琰", "崔志方", "达尔巴", "戴宗", "黛绮丝", "单廷珪", "单相思", "邓艾", "邓飞", "邓芝", "典韦", "店伴", "貂蝉", "丁得孙", "丁奉", "东方不败", "董平", "董袭", "董小姐", "董允", "董卓", "杜迁", "杜兴", "段景住", "段王爷", "段智兴", "法正", "樊瑞", "范坚强", "范遥", "费彬", "费点心", "费祎", "逢纪", "福康安", "傅红雪", "傅佥", "富察傅恒", "甘宁", "高乐高", "高沛", "高翔", "葛优躺", "公孙绿萼", "公孙胜", "公孙瓒", "公孙止", "龚旺", "苟不理", "古若般", "顾大嫂", "顾雍", "关平", "关胜", "关索", "关兴", "关羽", "管宁", "郭冬临", "郭芙", "郭嘉", "郭靖", "郭盛", "郭图", "郭襄", "郭攸之", "韩梅梅", "韩遂", "韩滔", "郝大通", "郝思文", "郝有钱", "何思豪", "何太冲", "何仙姑", "贺繁星", "贺啤酒", "洪七公", "侯健", "呼延灼", "胡斐", "胡青牛", "胡青羊", "胡一统", "扈三娘", "花荣", "华歆", "华英雄", "皇甫端", "黄权", "黄蓉", "黄信", "黄药师", "黄忠", "黄祖", "霍都", "霍峻", "纪梵希", "贾诩", "姜铁山", "姜维", "蒋道理", "蒋调侯", "蒋干", "蒋敬", "蒋钦", "蒋琬", "焦挺", "脚夫", "解宝", "解珍", "金大坚", "金拱门", "金花婆婆", "金轮法王", "沮授", "康师傅", "孔亮", "孔明", "孔融", "蒯越", "蓝月亮", "劳德诺", "乐和", "乐进", "雷横", "雷老虎", "雷同", "李拜天", "李典", "李衮", "李恢", "李俊", "李逵", "李立", "李莫愁", "李严", "李应", "李云", "李志常", "李忠", "梁长老", "廖化", "林朝英", "林冲", "林夫人", "林更新", "林平之", "林震南", "凌统", "凌振", "令狐冲", "刘巴", "刘备", "刘表", "刘禅", "刘处玄", "刘封", "刘鹤真", "刘唐", "刘焉", "刘璋", "刘正风", "娄阿鼠", "卢俊义", "鲁大师", "鲁肃", "鲁智深", "陆大有", "陆绩", "陆抗", "陆无双", "陆逊", "鹿清笃", "罗蛳粉", "骆统", "吕布", "吕洞宾", "吕方", "吕夫人", "吕蒙", "吕通", "吕威", "马超", "马春花", "马岱", "马冬梅", "马麟", "马腾", "马行空", "马钰", "马忠", "毛不易", "毛玠", "梅毛病", "孟达", "孟康", "苗人凤", "苗若兰", "灭绝师太", "莫大", "莫声谷", "慕容景岳", "穆春", "穆桂英", "穆弘", "南兰", "南仁通", "宁财神", "宁中则", "牛欢喜", "欧鹏", "欧阳锋", "潘桃园", "潘璋", "庞德", "庞统", "裴宣", "彭玘", "彭莹玉", "平四", "戚仙女", "戚长发", "祁志诚", "钱多多", "乔帮主", "谯周", "秦明", "丘处机", "裘千尺", "裘千仞", "曲非烟", "曲奇饼", "曲洋", "麴义", "全琮", "任我行", "任盈盈", "阮士中", "阮小二", "阮小七", "阮小五", "商宝震", "商老太", "审配", "盛淮南", "施不全", "施恩", "石万嗔", "石秀", "石勇", "时迁", "史进", "史珍香", "说不得", "司马师", "司马懿", "司马昭", "宋德方", "宋江", "宋青书", "宋清", "宋万", "宋小宝", "宋远桥", "苏大强", "孙不二", "孙綝", "孙大虎", "孙德崖", "孙二娘", "孙皓", "孙峻", "孙立", "孙亮", "孙鲁班", "孙鲁育", "孙权", "孙小虎", "孙新", "孙行者", "孙休", "索超", "谭处端", "汤达人", "汤隆", "唐老鸭", "陶气包", "陶谦", "陶子安", "陶宗旺", "田伯光", "田丰", "田归农", "田青文", "童猛", "童威", "万震山", "王定六", "王剑杰", "王剑英", "王朗", "王难姑", "王平", "王英", "王志坦", "王中王", "王重阳", "韦蝠王", "韦一笑", "卫无忌", "魏定国", "魏生金", "魏延", "吴班", "吴兰", "吴所谓", "吴彦祖", "吴懿", "吴用", "武敦儒", "武三通", "武松", "武修文", "夏侯霸", "夏侯惇", "夏侯尚", "夏侯玄", "夏侯渊", "夏一跳", "鲜于通", "向太阳", "向问天", "项充", "萧让", "小龙女", "小昭", "谢大脚", "谢逊", "辛毗", "辛评", "邢捕头", "徐晃", "徐宁", "徐盛", "徐铮", "许褚", "许三多", "许攸", "宣赞", "薛鹊", "薛永", "薛综", "荀攸", "荀彧", "严畯", "严颜", "言达平", "阎基", "燕青", "燕顺", "杨春", "杨巅峰", "杨过", "杨怀", "杨林", "杨逍", "杨雄", "杨仪", "杨志", "耶律齐", "一灯大师", "仪琳", "易中天", "殷吉", "殷离", "殷梨亭", "殷素素", "殷天正", "殷仲翔", "尹君子", "尹志平", "瑛姑", "游坦之", "于得水", "于禁", "俞岱岩", "俞莲舟", "虞翻", "郁保四", "袁绍", "袁术", "袁紫衣", "岳不群", "岳灵珊", "张苞", "张飞", "张郃", "张横", "张纮", "张辽", "张鲁", "张青", "张清", "张三丰", "张顺", "张松", "张松溪", "张温", "张无忌", "张嶷", "张翼", "张允", "张昭", "张志光", "张志敬", "张总管", "赵半山", "赵广", "赵敏", "赵统", "赵相机", "赵云", "赵志敬", "甄漂亮", "郑成功", "郑点钱", "郑天寿", "钟点工", "钟会", "钟兆能", "钟兆文", "钟兆英", "周伯通", "周仓", "周大福", "周颠", "周鲂", "周泰", "周通", "周瑜", "周芷若", "朱八戒", "朱富", "朱贵", "朱桓", "朱然", "朱仝", "朱武", "朱子柳", "诸葛瑾", "诸葛恪", "诸葛亮", "诸葛尚", "诸葛瞻", "邹润", "邹渊", "左冷禅"}

		''' <summary>英文姓名</summary>
		Public Shared NAMES_ENGLISH As String() = {"Abigail", "Adam", "Adelaide", "Adeline", "Adrian", "Aiden", "Aileen", "Aimee", "Alan", "Albert", "Alberto", "Alden", "Alex", "Alexander", "Alfred", "Alice", "Alicia", "Alvin", "Amanda", "Amelia", "Amy", "Anastasia", "Anders", "Andre", "Andrea", "Andrew", "Andy", "Angel", "Angela", "Angelina", "Ann", "Anna", "Anne", "Ansel", "Antonia", "Antony", "Apollo", "April", "Archie", "Aria", "Ariana", "Arianna", "Arnold", "Arthur", "Ashley", "Audrey", "August", "Aurora", "Austin", "Autumn", "Ava", "Avery", "Axel", "Baldwin", "Barry", "Barton", "Basil", "Beau", "Beck", "Belinda", "Bella", "Ben", "Benjamin", "Bennett", "Bert", "Bertha", "Bertram", "Beverly", "Bill", "Billy", "Blake", "Bob", "Bobby", "Bonnie", "Brad", "Bradley", "Brady", "Brandon", "Brant", "Brayden", "Brenda", "Brett", "Brian", "Bridget", "Brittany", "Bruce", "Bryce", "Caleb", "Calvin", "Cameron", "Camille", "Candice", "Carey", "Carl", "Carlo", "Carlos", "Carmen", "Carol", "Caroline", "Carolyn", "Carter", "Casey", "Cathy", "Cecilia", "Cedric", "Chad", "Chance", "Chanel", "Charles", "Charlie", "Charlotte", "Cherry", "Cheryl", "Chester", "Chris", "Christian", "Christina", "Christine", "Christopher", "Claire", "Clara", "Clarence", "Clarissa", "Clark", "Claude", "Claudia", "Clay", "Cliff", "Clifton", "Coco", "Cody", "Colby", "Cole", "Colin", "Colton", "Conner", "Connie", "Connor", "Constance", "Cooper", "Corey", "Cory", "Craig", "Crystal", "Curt", "Curtis", "Cynthia", "Cyril", "Daisy", "Dale", "Dallas", "Damian", "Damon", "Dan", "Dane", "Daniel", "Danielle", "Danny", "Daphne", "Darren", "Daryl", "Dave", "David", "Davin", "Dawson", "Dean", "Deborah", "Debra", "Delia", "Dennis", "Derek", "Desmond", "Devin", "Dexter", "Diana", "Diane", "Dillon", "Dirk", "Don", "Donald", "Donovan", "Doris", "Dorothy", "Doug", "Douglas", "Drew", "Duane", "Dustin", "Dylan", "Earl", "Easton", "Ed", "Eddie", "Edgar", "Edison", "Edith", "Edmond", "Edmund", "Edna", "Edward", "Edwin", "Edwina", "Egan", "Eileen", "Elaine", "Elena", "Eli", "Elias", "Elijah", "Elizabeth", "Ella", "Ellen", "Ellie", "Elliot", "Elsa", "Elsie", "Elvis", "Emil", "Emily", "Emma", "Emmanuel", "Emmett", "Eric", "Erica", "Erin", "Ernest", "Ervin", "Esther", "Ethan", "Eugene", "Eva", "Evan", "Evelyn", "Everett", "Fabian", "Finn", "Fiona", "Fletcher", "Flora", "Florence", "Ford", "Frances", "Francis", "Frank", "Franklin", "Fred", "Freda", "Freddie", "Frederick", "Gabriel", "Gabriella", "Gage", "Gail", "Galen", "Garrett", "Gary", "Gavin", "Gemma", "Gene", "Geoffrey", "George", "Georgia", "Gerald", "Gerry", "Gertrude", "Gibson", "Gilbert", "Gillian", "Gina", "Gladys", "Glen", "Glenn", "Gloria", "Gordon", "Grace", "Grady", "Grant", "Gray", "Greg", "Gregory", "Gunnar", "Gus", "Guy", "Gwendolyn", "Hank", "Hanna", "Hannah", "Hardy", "Harley", "Harold", "Harriet", "Harrison", "Harry", "Hayden", "Heath", "Heather", "Hector", "Hedy", "Helen", "Helena", "Henrietta", "Henry", "Herman", "Hilton", "Hiram", "Holden", "Holly", "Hope", "Howard", "Hudson", "Hugh", "Hugo", "Humphrey", "Ian", "Ida", "Ignacio", "Ike", "Irene", "Iris", "Isaac", "Isabel", "Isabella", "Isiah", "Ivan", "Ivy", "Jace", "Jack", "Jackson", "Jacqueline", "Jake", "James", "Jamie", "Jane", "Janet", "Jarred", "Jarrett", "Jasmine", "Jason", "Jay", "Jayden", "Jean", "Jeanne", "Jeff", "Jeffrey", "Jenna", "Jennifer", "Jenny", "Jeremiah", "Jeremy", "Jesse", "Jessica", "Jett", "Jewel", "Jill", "Jim", "Jimmy", "Joan", "Joanna", "Joanne", "Jodie", "Jody", "Joel", "John", "Johnny", "Jolene", "Jon", "Jonathan", "Jordon", "Jorge", "Jose", "Joseph", "Josephine", "Josh", "Joshua", "Josiah", "Joy", "Joyce", "Judy", "Julia", "Julian", "Juliana", "Julie", "June", "Justin", "Kale", "Kaleb", "Kane", "Karen", "Karl", "Kate", "Katherine", "Kathleen", "Kathryn", "Kathy", "Katie", "Keira", "Keith", "Kelly", "Kelvin", "Ken", "Kendall", "Kendra", "Kennedy", "Kenneth", "Kenny", "Kent", "Kerry", "Kevin", "Kian", "Kieran", "Kim", "Kimberly", "King", "Kirk", "Kitty", "Kristin", "Kristy", "Kyle", "Lacey", "Lamar", "Lamont", "Lana", "Lance", "Landry", "Lane", "Lara", "Larissa", "Larry", "Latoya", "Laura", "Lauren", "Lawrence", "Leah", "Leander", "Leanne", "Lee", "Leila", "Lena", "Leo", "Leon", "Leonard", "Leonora", "Leroy", "Les", "Lesley", "Leslie", "Lester", "Lia", "Liam", "Lillian", "Lily", "Lincoln", "Linda", "Lindsay", "Lisa", "Liz", "Liza", "Lloyd", "Logan", "Lois", "Lola", "Lolita", "Lonnie", "Loren", "Lorenzo", "Lori", "Louis", "Louise", "Lucas", "Lucia", "Lucian", "Lucille", "Lucy", "Lulu", "Luther", "Lydia", "Lyle", "Lynn", "Mabel", "Mack", "Madden", "Maddox", "Madison", "Maggie", "Malcolm", "Malik", "Mandy", "Manning", "Marc", "Marcel", "Marco", "Marcus", "Marek", "Margaret", "Maria", "Marian", "Marianne", "Marie", "Marilyn", "Marina", "Mario", "Marion", "Marissa", "Mark", "Marley", "Marlon", "Marshall", "Martha", "Martin", "Martina", "Marvin", "Mary", "Mason", "Mathew", "Matthew", "Maurice", "Max", "Maxwell", "Mayer", "Megan", "Melanie", "Melissa", "Melvin", "Meredith", "Merrill", "Merry", "Meyer", "Michael", "Michelle", "Mickey", "Mike", "Mildred", "Miles", "Millie", "Milton", "Mitchell", "Molly", "Monica", "Monty", "Morgan", "Morris", "Morton", "Moses", "Myles", "Myrna", "Nadia", "Nancy", "Naomi", "Natalie", "Natalya", "Nathan", "Nathaniel", "Neil", "Nelson", "Neville", "Newton", "Nicholas", "Nick", "Nico", "Nicola", "Nicole", "Nigel", "Nina", "Noah", "Noel", "Noelle", "Norma", "Norman", "Norris", "Oliver", "Olivia", "Ophelia", "Oscar", "Owen", "Pablo", "Paige", "Pamela", "Parker", "Pat", "Patricia", "Patrick", "Patty", "Paul", "Paula", "Pauline", "Payson", "Payton", "Pearl", "Peggy", "Penelope", "Perry", "Peter", "Petra", "Peyton", "Phil", "Philip", "Phoebe", "Phyllis", "Pierce", "Polly", "Portia", "Priscilla", "Quentin", "Quincy", "Rachael", "Rachel", "Rae", "Raina", "Ralph", "Ramona", "Randall", "Randolph", "Raymond", "Rebecca", "Regina", "Renata", "Renee", "Richard", "Rita", "Robert", "Roberta", "Robin", "Rochelle", "Roger", "Ronald", "Rose", "Rosemary", "Rosie", "Roxanne", "Ruby", "Ruth", "Ryan", "Sabrina", "Sally", "Sam", "Samanta", "Samantha", "Samuel", "Sandra", "Sara", "Sarah", "Scarlett", "Scott", "Sean", "Selena", "Selina", "Serena", "Shannon", "Sharon", "Shawn", "Sheila", "Shelley", "Shelly", "Sherry", "Shirley", "Simon", "Sophia", "Sophie", "Stella", "Stephanie", "Stephen", "Steve", "Sue", "Susan", "Suzanne", "Sylvia", "Tamara", "Tanya", "Tara", "Tasha", "Teresa", "Thomas", "Tiffany", "Tim", "Tina", "Tracy", "Trevor", "Trudy", "Ursula", "Valerie", "Vanessa", "Vera", "Veronica", "Vicki", "Victor", "Victoria", "Vincent", "Virginia", "Vivian", "Wanda", "Wayne", "Wendy", "Whitney", "William", "Wilma", "Yvonne", "Zachary", "Zara", "Zoe"}

#End Region

		''' <summary>随机数生成器</summary> 
		Private Shared _Rnd As System.Random

		''' <summary>随机数生成器</summary> 
		Public Shared ReadOnly Property Rnd As System.Random
			Get
				If _Rnd Is Nothing Then
					' 使用RNGCryptoServiceProvider 做种，可以在一秒内产生的随机数重复率非常的低，对于以往使用时间做种的方法是个升级
					Dim RndBytes = New Byte(3) {}

					Using RNG = Security.Cryptography.RandomNumberGenerator.Create
						RNG.GetBytes(RndBytes)
					End Using

					Dim Seed = BitConverter.ToInt32(RndBytes, 0)

					_Rnd = New System.Random(Seed)
				End If
				Return _Rnd
			End Get
		End Property

		''' <summary>获取指定长度随机的数字字符串</summary> 
		''' <param name="length">待获取随机字符串长度</param> 
		Public Shared Function Number(length As Integer) As String
			Return Make(NUMBER_CHARS, length)
		End Function

		''' <summary>获取指定长度随机的字母字符串（包含大小写字母）</summary> 
		''' <param name="length">待获取随机字符串长度</param> 
		Public Shared Function Chars(length As Integer) As String
			Return Make(LOWER_CHARS & UPPER_CHARS, length)
		End Function

		''' <summary>获取指定长度随机的字母＋数字混和字符串（包含大小写字母）</summary> 
		Public Shared Function Mix(length As Integer) As String
			Return Make(LOWER_CHARS & UPPER_CHARS & NUMBER_CHARS, length)
		End Function

		''' <summary>从指定字符串中抽取指定长度的随机字符串</summary> 
		''' <param name="source">源字符串</param> 
		''' <param name="length">待获取随机字符串长度</param>
		Public Shared Function Make(source As String, length As Integer) As String
			Dim Value = ""

			If source?.Length > 0 Then
				length = length.Range(1, 1024)

				With New StringBuilder
					For I = 0 To length - 1
						Dim path = Rnd.Next(0, source.Length)
						.Append(source.AsSpan(path, 1))
					Next

					Value = .ToString
				End With
			End If

			Return Value
		End Function

		''' <summary>GUID</summary>
		Public Shared Function Guid() As String
			Return System.Guid.NewGuid.ToString
		End Function

		''' <summary>16位字符串</summary>
		Public Shared Function Hash() As String
			Return Guid.MD5(False)
		End Function

		''' <summary>随机产生常用汉字</summary>
		''' <param name="length">要产生汉字的个数</param>
		''' <returns>常用汉字</returns>
		''' <remarks>
		''' 随机中文汉字验证码的基本原理:汉字区位码表区位码、国标码与机内码的转换关系
		''' 1）区位码先转换成十六进制数表示
		''' 2）（区位码的十六进制表示）＋2020H＝国标码；
		''' 3）国标码＋8080H＝机内码
		''' 举例：以汉字“大”为例，“大”字的区内码为20831、区号为20，位号为832、将区位号2083转换为十六进制表示为1453H3、1453H＋2020H＝3473H，得到国标码3473H4、3473H＋8080H＝B4F3H，得到机内码为B4F3H
		''' 常用汉字在16-55区,其中55区有几个空的,故要将其去除.
		''' </remarks>
		Public Shared Function ChineseWords(length As Integer) As String
			Dim Value As String = ""
			length = length.Range(1, 1024)

			With New StringBuilder
				For I As Integer = 1 To length
					'获取区码(常用汉字的区码范围[fan wei]为16-55)
					Dim regionCode As Integer = Rnd.Next(16, 56)

					'获取位码(位码范围[fan wei]为1-94 由于55区的90,91,92,93,94为空,故将其排除)
					Dim positionCode As Integer

					If regionCode > 54 Then
						'55区排除 90,91,92,93,94
						positionCode = Rnd.Next(1, 90)
					Else
						positionCode = Rnd.Next(1, 95)
					End If

					' 转换区位码为机内码
					regionCode += 160
					positionCode += 160

					' 转换为汉字
					.Append(GB2312.GetString({CByte(regionCode), CByte(positionCode)}))
				Next

				Value = .ToString
			End With

			Return Value
		End Function

		''' <summary>随机产生名称</summary>
		Public Shared Function Name(Optional isEn As Boolean = False) As String
			Dim names = If(isEn, NAMES_ENGLISH, NAMES_CHINESE)
			If names.IsEmpty Then Return ""

			Dim path = Rnd.Next(0, names.Length)
			Return names(path)
		End Function
	End Class

End Namespace
