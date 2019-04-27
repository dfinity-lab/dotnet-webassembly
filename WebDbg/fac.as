func fac(n : Nat) : Nat {
  if (n == 0)  {
    assert(false);
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
  let some : ? Int = ? 1;
  let text = "hello";
  let array = [ 1, 2, 3];
  let tuple = (false, 1, "hello");
  let obj = new {x = 1; y = 2};
  let variant = #node;
  let f = func () {};
};

let _ = display();

let res = fac 20;
