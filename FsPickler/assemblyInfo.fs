﻿namespace FsPickler

    open System.Reflection

#if BUILD_STRONG_NAME
    [<assembly:AssemblyKeyFile("../../Lib/key.snk")>]
#endif
    [<assembly:AssemblyVersion("0.8.3.*")>]
    do()


    module internal Config =
        
        [<Literal>]
        let optimizeForLittleEndian = true