# IQueryable

背景:自定义的IEnumerable的扩展方法用在IQuerable对象上时,编译没问题,但是在运行时会提示语句无法翻译

因此:自定义的IEnumerable的扩展方法不能用在IQuerable对象上.

具体场景: 如EF的 IQuerable , SqlSugar 的 SqlSugar.ISugarQueryable



# Enum

背景: 原本的枚举对象是int值, 代码使用 .Equals()来判断, 在重构后int类型变成了枚举,此时用Equals方法进行判断时, 结果为 false.

该问题产生后: 编译没问题,运行也不会报错, 只有在业务中发现了才会被排查出来, 然后代码的排查过程也没有具体的好方法

因此:禁止使用`枚举对象.Equals()`来进行值比较判断



# IActionResult 

背景:在Service中返回了 IActionResult对象

```
  public async Task<FileStreamResult> DownloadFileByType(string filePath, string fileDownLoadName)
  {
      await Task.Delay(1); //解决此异步方法缺少 "await" 运算符的提示

      if (File.Exists(filePath) == false)
      {
          return default; //IActionResult 不能返回null , 否则产生异常: Cannot return null from an action method with a return type of 'Microsoft.AspNetCore.Mvc.IActionResult'.

          // 返回 一个空白的 PNG 图片（1x1 像素，透明） 的 ileStreamResult
          return new FileStreamResult(new MemoryStream(new byte[] {/*...省略...*/ }), "image/png")
          {
              FileDownloadName = "empty.png"
          };
      }

      return default;
  }
```

因此: IActionResult 的方法不能返回null

