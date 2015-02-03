// MessengerUnitTest.cs v1.0 by Magnus Wolffelt, magnus.wolffelt@gmail.com
//
// Delegates used in Messenger.cs.

public delegate void Callback ();

public delegate void Callback<T> (T arg1);

public delegate void Callback<T,U> (T arg1,U arg2);

public delegate void Callback<T,U,V> (T arg1,U arg2,V arg3);

public delegate void Callback<T,U,V,K> (T arg1,U arg2,V arg3,K arg4);

public delegate void Callback<T,U,V,K,Z> (T arg1,U arg2,V arg3,K arg4,Z arg5);

public delegate void Callback<T,U,V,K,Z,X,Y> (T arg1,U arg2,V arg3,K arg4,Z arg5,X arg6,Y arg7);