

func swap(a : [var Int], i : Nat, j : Nat) {
  let temp = a[i];
  a[i] := a[j];
  a[j] := temp;
};

func partition(a : [var Int], lo : Nat, hi : Nat) : Nat {
  let pivot = a[lo];
  var i : Nat = lo;
  var j : Nat = hi;

  loop {
    while (a[i] < pivot) i += 1;
    while (a[j] > pivot) j -= 1;
    if (i >= j) return j;
    swap(a, i, j);
  };
};

func quicksort(a : [var Int], lo : Nat, hi : Nat) {
  if (lo < hi) {
    let p = partition(a, lo, hi);
	  quicksort(a, lo, p);
	  quicksort(a, p + 1, hi);
	};
};




func fac(n : Nat) : Nat {
  if (n == 0)  {
    return(1);
  }
  else {
    n *
    fac (n-1);
  };
};



func display () {
  let six = 6;
  let truth = true;
  let none : ? Int = null;
  let some : ? Int = ? 666;
  let text = "hello";
  let array = [ 1, 2, 3];
  let tuple = (false, 1, "hello");
  let obj = new {x = 1; y = 2};
  let variant = #node (#leaf "left", #leaf "right");
  let f = fac;
  let _ = f 1; // bug?
};

let _ = display();

let res = fac 6;


let a : [var Int] = [var 8, 3, 9, 5, 2];

quicksort(a, 0, 4);
