
module Fs.Runner.Main

type 'a Tree =
    | Node of 'a * Tree<'a> * Tree<'a>
    | Empty


type ('a, 'acc) Visitor = 'acc -> 'a -> Tree<'a> -> Tree<'a> -> 'acc

let rec traverse f tree =
    match tree with
    | Node (elem, left, right) ->
        f elem; traverse f left; traverse f right
    | Empty -> ()

let tree1 = Node(1, Node(2, Empty, Empty), Node(3, Empty, Empty))


let append_front (lref:'a list ref) node =
  lref.Value <- node :: lref.Value

let append_back (lref:'a list ref) node =
  lref.Value <- List.append lref.Value [node]


let preorder<'a> (lref:'a list ref) = 
    fun node -> append_back lref node


let postorder<'a> (lref:'a list ref) = 
    fun node -> append_back lref node


let rec traverse_inorder f tree = 
    match tree with
    | Node (elem, left, right) ->
        traverse_inorder f left; f elem; traverse_inorder f right
    | Empty -> ()


let rec traverse_acc (f: Visitor<'a, 'acc>) (acc: 'acc) tree = 
    match tree with
    | Node (elem, left, right) -> f acc elem left right
    | Empty -> acc

let rec inorder: Visitor<'a, 'a list> =
    fun acc eleme left right ->
        traverse_acc inorder acc left @ [eleme] @ traverse_acc inorder acc right

let rec preorder_visitor: Visitor<'a, 'a list> =
    fun acc elem left right ->
        [elem] @ traverse_acc preorder_visitor acc left @ traverse_acc preorder_visitor acc right

let () =
    printfn "%A" (traverse_acc preorder_visitor [] tree1)
